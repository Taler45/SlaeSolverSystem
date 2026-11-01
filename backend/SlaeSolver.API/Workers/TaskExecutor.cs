using Microsoft.Extensions.Options;
using SlaeSolver.BLL.Options;
using SlaeSolver.BLL.Solver;
using SlaeSolver.BLL.Storage;
using SlaeSolver.Domain.Repositories;

namespace SlaeSolver.API.Workers;

public class TaskExecutor: BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServerOptions _serverOptions;
    private readonly LimitsOptions _limitsOptions;
    private readonly SolverOptions _solverOptions;
    private readonly ILogger<TaskExecutor> _logger;
    private readonly SemaphoreSlim _slots;

    public TaskExecutor(IServiceScopeFactory scopeFactory, IOptions<ServerOptions> serverOptions,
        IOptions<LimitsOptions> limitsOptions, IOptions<SolverOptions> solverOptions, ILogger<TaskExecutor> logger)
    {
        _scopeFactory = scopeFactory;
        _limitsOptions = limitsOptions.Value;
        _solverOptions = solverOptions.Value;
        _serverOptions = serverOptions.Value;
        _logger = logger;
        _slots = new SemaphoreSlim(_serverOptions.MaxConcurrentTasks);

    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервер {ServerId}: виконавець запущено, слотів {Slots}",
            _serverOptions.ServerId, _serverOptions.MaxConcurrentTasks);
        await RecoverStuckTasksAsync();
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_serverOptions.PollIntervalSeconds));

        try
        {
            do
            {
                await TakeTasksWhileSlotsFreeAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));

        }
        catch (OperationCanceledException)
        {
            
        }
        
    }

    private async Task TakeTasksWhileSlotsFreeAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _slots.CurrentCount > 0)
        {
            await _slots.WaitAsync(stoppingToken);

            Guid? taskId;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

                taskId = await repository.ClaimNextQueuedAsync(_serverOptions.ServerId);
            }
            catch (Exception ex)
            {
                _slots.Release();
                _logger.LogError(ex, "Не вдалося взяти задачу з черги");
                return;
            }

            if (taskId is null)
            {
                _slots.Release();
                return;
            }

            _logger.LogInformation("Сервер {ServerId}: взято задачу {TaskId}", _serverOptions.ServerId, taskId);

            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessTaskAsync(taskId.Value, stoppingToken);
                }
                finally
                {
                    _slots.Release();
                }
            });
        }
    }

    private async Task RecoverStuckTasksAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

            var stuck = await repository.GetInProgressByServerAsync(_serverOptions.ServerId);
            foreach (var task in stuck)
            {
                task.Fail("Обчислення перервано: сервер було перезапущено");
            }

            if (stuck.Any())
            {
                await repository.SaveChangesAsync();
                _logger.LogWarning("Сервер {ServerId}: завислі задачі позначено помилкою", _serverOptions.ServerId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не вдалося відновити завислі задачі");
        }

    }



  private async Task ProcessTaskAsync(Guid taskId, CancellationToken stoppingToken)
{
    using var scope = _scopeFactory.CreateScope();

    var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
    var fileHandler = scope.ServiceProvider.GetRequiredService<IFileStorage>();

    var task = await repository.GetByIdAsync(taskId);
    if (task is null)
    {
        return;
    }

    var state = new ExecutionState();
    using var solveCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
    solveCts.CancelAfter(TimeSpan.FromSeconds(_limitsOptions.MaxSolveSeconds));
    using var monitorCts = new CancellationTokenSource();

    Task? monitor = null;

    try
    {
        var parametrs = await fileHandler.ReadParametersAsync(task.ParameterFilePath);
        monitor = MonitorAsync(taskId, state, solveCts, monitorCts.Token);

        var result = JacobiSolver.Solve(parametrs, percent => state.Progress = percent, solveCts.Token, _solverOptions);

        if (result.Converged)
        {
            string resultPath = await fileHandler.SaveResultAsync(taskId, result);
            task.Complete(resultPath);
        }
        else
        {
            task.Fail($"Метод не зійшовся за {result.Iterations} ітерацій");
        }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
        task.Fail("Обчислення перервано: сервер зупинено");
    }
    catch (OperationCanceledException)
    {
        if (state.CancelRequested)
        {
            task.MarkCanceled();
        }
        else
        {
            task.Fail($"Перевищено ліміт часу обчислення ({_limitsOptions.MaxSolveSeconds} с)");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Помилка під час обчислення задачі {TaskId}", taskId);
        task.Fail(ex.Message);
    }
    finally
    {
        monitorCts.Cancel();
        if (monitor is not null)
        {
            await monitor;
        }
    }
    try
    {
        await repository.SaveChangesAsync();
        _logger.LogInformation("Задача {TaskId} завершена зі статусом {Status}", taskId, task.Status);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Не вдалося зберегти результат задачі {TaskId}", taskId);
    }
}

    private async Task MonitorAsync(Guid taskId, ExecutionState state, CancellationTokenSource solveCts,
        CancellationToken monitorToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_serverOptions.ProgressSaveSeconds));
        try
        {
            while (await timer.WaitForNextTickAsync(monitorToken))
            {
                await repository.UpdateProgressAsync(taskId, state.Progress);
                bool isCancel = await repository.IsCancelRequestedAsync(taskId);
                if (isCancel)
                {
                    state.CancelRequested = true;
                    solveCts.Cancel();
                    return;
                }

            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Помилка моніторингу задачі {TaskId}", taskId);
        }
    }
    


    private class ExecutionState
    {
        public volatile int Progress;
        public volatile bool CancelRequested;
    }
}
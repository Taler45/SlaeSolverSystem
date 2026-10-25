using System.ComponentModel;
using Microsoft.Extensions.Options;
using SlaeSolver.BLL.DTOs.Tasks;
using SlaeSolver.BLL.Models;
using SlaeSolver.BLL.Options;
using SlaeSolver.BLL.Solver;
using SlaeSolver.BLL.Storage;
using SlaeSolver.Domain.Entities;
using SlaeSolver.Domain.Exceptions;
using SlaeSolver.Domain.Repositories;

namespace SlaeSolver.BLL.Services.Tasks;

public class TaskService: ITaskService
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskRepository _taskRepository;
    private readonly LimitsOptions _options;

    public TaskService(IFileStorage storage, ITaskRepository taskRepository, IOptions<LimitsOptions> options)
    {
        _options = options.Value;
        _fileStorage = storage;
        _taskRepository = taskRepository;
    }
    
    
    public async Task<TaskDto> CreateAsync(Guid userId, CreateTaskRequest request)
    {
        if (request.MatrixSize <= 0)
        {
            throw new DomainException("Кількість невідомих має бути більшою за нуль");
        }

        if (request.MatrixSize > _options.MaxMatrixSize)
        {
            throw new LimitExceededException($"Максимальна кількість невідомих: {_options.MaxMatrixSize}");
        }
        
        if (await _taskRepository.CountActiveByUserAsync(userId) >= _options.MaxActiveTasks)
        {
            throw new LimitExceededException($"Максимальна кількість активних задач: {_options.MaxActiveTasks}");
        }
        
        TaskParameters parameters;

        if (request.Matrix is null && request.RightSide is null)
        {
            parameters = SystemGenerator.Generate(request.MatrixSize);
        }
        else if (request.Matrix is not null && request.RightSide is not null)
        {
            ValidateSystem(request.MatrixSize, request.Matrix, request.RightSide);
            parameters = new TaskParameters(request.MatrixSize, request.Matrix, request.RightSide);
        }
        else
        {
            throw new DomainException("Потрібно передати і матрицю, і праву частину, або нічого");
        }
        
        var taskId = Guid.NewGuid();
        var path = await _fileStorage.SaveParametersAsync(taskId, parameters);

        var task = TaskEntitie.Create(taskId, userId, request.MatrixSize, path);
        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return new TaskDto(task.Id,task.MatrixSize,task.Status.ToString(),task.Progress,
            task.IsCanceled,task.ErrorMessage,task.CreatedAt,task.StartedAt,task.CompletedAt);
    }

    private static void ValidateSystem(int n, double[][] matrix, double[] rightSide)
    {
        if (matrix.Length != n)
        {
            throw new DomainException($"Матриця має містити {n} рядків");
        }

        foreach (var row in matrix)
        {
            if (row is null || row.Length != n)
            {
                throw new DomainException($"Кожен рядок матриці має містити {n} елементів");
            }

            foreach (var value in row)
            {
                if (!double.IsFinite(value))
                {
                    throw new DomainException("Матриця містить некоректні числа");
                }
            }
        }

        if (rightSide.Length != n)
        {
            throw new DomainException($"Права частина має містити {n} елементів");
        }

        foreach (var value in rightSide)
        {
            if (!double.IsFinite(value))
            {
                throw new DomainException("Права частина містить некоректні числа");
            }
        }
    }
    public async Task<TaskDto> GetAsync(Guid userId, Guid taskId)
    {
        TaskEntitie? task = await _taskRepository.GetByIdForUserAsync(taskId,userId);

        if (task is null)
        {
            throw new NotFoundException("такої таски не було знайдено");
        }
        return new TaskDto(task.Id,task.MatrixSize,task.Status.ToString(),task.Progress,
            task.IsCanceled,task.ErrorMessage,task.CreatedAt,task.StartedAt,task.CompletedAt);
    }

    public async Task<IEnumerable<TaskDto>> GetHistoryAsync(Guid userId, int page, int pageSize)
    {
        if (page < 1)
        {
            throw new DomainException("Номер сторінки має бути не меншим за 1");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new DomainException("Розмір сторінки має бути від 1 до 100");
        }
        IEnumerable<TaskEntitie> tasks = await _taskRepository.GetByUserAsync(userId, page, pageSize);

        var taskDtos = tasks.Select(t => new TaskDto(
            t.Id,
            t.MatrixSize,
            t.Status.ToString(),
            t.Progress,
            t.IsCanceled,
            t.ErrorMessage,
            t.CreatedAt,
            t.StartedAt,
            t.CompletedAt
        ));
        return taskDtos;
    }

    public async Task<TaskDto> CancelAsync(Guid userId, Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        
        if (task is null || task.UserId != userId)
        {
            throw new NotFoundException("Задачу не знайдено");
        }
        
        task.RequestCancel();
        
        await _taskRepository.SaveChangesAsync();

        return new TaskDto(task.Id,task.MatrixSize,task.Status.ToString(),task.Progress,
            task.IsCanceled,task.ErrorMessage,task.CreatedAt,task.StartedAt,task.CompletedAt);
    }
}
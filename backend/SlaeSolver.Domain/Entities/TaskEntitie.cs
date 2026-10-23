using SlaeSolver.Domain.Exceptions;

namespace SlaeSolver.Domain.Entities;

public class TaskEntitie
{
    public const int PATH_MAX_LENGTH = 500;
    public const int SERVER_ID_MAX_LENGTH = 50;
    public Guid Id { get; private set; }
    public string ParameterFilePath { get; private set; } = string.Empty;
    public int MatrixSize { get; private set; }
    public CalculationStatus Status { get; private set; }
    public string? ServerId { get; private set; }
    public int Progress { get; private set; }
    public bool IsCanceled { get; private set; }
    public string? ResultFilePath { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid UserId { get; private set; }
    public UserEntitie User { get; private set; } = null!;

    public string? ErrorMessage { get; private set; }

    public bool IsActive => Status == CalculationStatus.Queued || Status == CalculationStatus.InProgress;

    private TaskEntitie() { }

    private TaskEntitie(Guid id, Guid userId, int matrixSize, string parameterFilePath)
    {
        Id = id;
        UserId = userId;
        ParameterFilePath = parameterFilePath;
        Progress = 0;
        Status = CalculationStatus.Queued;
        MatrixSize = matrixSize;
        CreatedAt = DateTime.UtcNow;
        IsCanceled = false;
    }

    public static TaskEntitie Create(Guid id, Guid userId, int matrixSize, string parameterFilePath)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Некоректний ідентифікатор користувача");
        }
        if (matrixSize <= 0)
        {
            throw new DomainException("Кількість невідомих має бути більшою за нуль");
        }
        if (string.IsNullOrWhiteSpace(parameterFilePath) || parameterFilePath.Length > TaskEntitie.PATH_MAX_LENGTH)
        {
            throw new DomainException("Некоректний шлях до файлу параметрів");
        }

        return new TaskEntitie(id, userId, matrixSize, parameterFilePath);
    }

    public void Start(string serverId)
    {
        if (Status != CalculationStatus.Queued)
        {
            throw new DomainException($"Неможна запускати задачу зі статусом {Status}");
        }

        Status = CalculationStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        ServerId = serverId;
        
    }

    public void UpdateProgress(int percent)
    {
        if (Status != CalculationStatus.InProgress)
        {
            throw new DomainException($"Не можна оновити прогрес задачі зі статусом {Status}");
        }

        percent = Math.Clamp(percent, 0, 100);
        if (percent > Progress)
        {
            Progress = percent; 
        }
        
    }
    
    public void RequestCancel()
    {
        if (!IsActive)
        {
            throw new DomainException("Задача вже завершена і не може бути скасована");
        }
        IsCanceled = true;
        if (Status == CalculationStatus.Queued)
        {
            Status = CalculationStatus.Canceled;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void MarkCanceled()
    {
        if (Status != CalculationStatus.InProgress)
        {
            throw new DomainException($"Не можна зупинити задачу зі статусом {Status}");
        }

        Status = CalculationStatus.Canceled;
        IsCanceled = true;
        CompletedAt = DateTime.UtcNow;
        
    }


    public void Complete(string resultFilePath)
    {
        if (Status != CalculationStatus.InProgress)
        {
            throw new DomainException($"Не можна завершити задачу зі статусом {Status}");
        }
        Progress = 100;
        Status = CalculationStatus.Done;
        CompletedAt = DateTime.UtcNow;
        ResultFilePath = resultFilePath;
    }

    public void Fail(string message)
    {
        if (!IsActive)
        {
            throw new DomainException($"Не можна позначити помилкою задачу зі статусом {Status}");
        }

        ErrorMessage = message;
        Status = CalculationStatus.Error;
        CompletedAt = DateTime.UtcNow;
    }
}
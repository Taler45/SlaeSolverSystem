namespace SlaeSolver.Domain.Entities;

public class TaskEntitie
{
    public Guid Id { get; private set; }
    public string ParameterFilePath { get;private set; }
    public int MatrixSize { get; private set; }
    public CalculationStatus Status { get;private set; }
    public string? ServerId { get;private set; }
    public int Progress { get;private set; }
    public bool IsCanceled { get;private set; }
    public string? ResultFilePath { get;private set; }
    public DateTime CreatedAt { get;private set; }
    public DateTime? StartedAt { get;private set; }
    public DateTime? CompletedAt { get;private set; }
    public Guid UserId { get;private set; }
    public UserEntitie User { get;private set; }

}
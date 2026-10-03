namespace SlaeSolver.Domain.Entities;

public class TaskEntitie
{
    public Guid Id { get; set; }
    public string ParameterFilePath { get; set; }
    public int MatrixSize { get; set; }
    public TaskStatus Status { get; set; }
    public string? ServerId { get; set; }
    public int Progress { get; set; }
    public bool IsCanceled { get; set; }
    public string? ResultFilePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid UserId { get; set; }
    public UserEntitie User { get; set; }

}
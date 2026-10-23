namespace SlaeSolver.Domain;

public enum CalculationStatus
{
    Queued,
    InProgress,
    Done,
    Canceled,
    Error
}
namespace SlaeSolver.BLL.DTOs.Tasks;

public record TaskDto(
    Guid Id,
    int MatrixSize,
    string Status,
    int Progress,
    bool IsCanceled,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt
    );
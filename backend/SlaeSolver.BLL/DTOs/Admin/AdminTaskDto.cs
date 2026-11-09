namespace SlaeSolver.BLL.DTOs.Admin;

public record AdminTaskDto(
    Guid Id,
    string UserEmail,
    int MatrixSize,
    string Status,
    int Progress,
    string? ServerId,
    DateTime CreatedAt
    );
namespace SlaeSolver.BLL.DTOs;

public record AuthRequest(
    string? Email,
    string? Password
);
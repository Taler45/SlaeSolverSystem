namespace SlaeSolver.BLL.DTOs.Auth;

public record AuthRequest(
    string? Email,
    string? Password
);
namespace SlaeSolver.BLL.DTOs.Auth;

public record RegisterRequest
(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Password
);
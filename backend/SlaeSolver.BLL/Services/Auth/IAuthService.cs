using SlaeSolver.BLL.DTOs;

namespace SlaeSolver.BLL.Services.Auth;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest registerRequest);
    Task<string> AuthAsync(AuthRequest authRequest);
}
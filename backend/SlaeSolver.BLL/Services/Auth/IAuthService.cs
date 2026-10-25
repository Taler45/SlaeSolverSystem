using SlaeSolver.BLL.DTOs.Auth;

namespace SlaeSolver.BLL.Services.Auth;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest registerRequest);
    Task<string> AuthAsync(AuthRequest authRequest);
}
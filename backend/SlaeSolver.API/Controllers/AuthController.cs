using Microsoft.AspNetCore.Mvc;
using SlaeSolver.BLL.DTOs;
using SlaeSolver.BLL.Services.Auth;

namespace SlaeSolver.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;


    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await _authService.RegisterAsync(request);
        return Ok(new { message = "Користувача успішно зареєстровано"});
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        var token = await _authService.AuthAsync(request);
        return Ok(new { token });
    }
    
    
}
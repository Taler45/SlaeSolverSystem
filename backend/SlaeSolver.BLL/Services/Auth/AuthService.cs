using SlaeSolver.BLL.DTOs.Auth;
using SlaeSolver.BLL.Security;
using SlaeSolver.Domain.Entities;
using SlaeSolver.Domain.Exceptions;
using SlaeSolver.Domain.Repositories;

namespace SlaeSolver.BLL.Services.Auth;

public class AuthService: IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJWTProvider _jwtProvider;

    public AuthService(IPasswordHasher passwordHasher, IJWTProvider jwtProvider, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }
    
    public async Task RegisterAsync(RegisterRequest registerRequest)
    {
        if (string.IsNullOrWhiteSpace(registerRequest.Email))
        {
            throw new DomainException("email порожній");
        }
        if (string.IsNullOrWhiteSpace(registerRequest.Password) 
            || registerRequest.Password .Length < 8 
            || registerRequest.Password.Length > 100)
        {
            throw new DomainException("Пароль має містити від 8 до 100 символів");
        }
        var isFound = await _userRepository.ExistsByEmailAsync(UserEntitie.NormalizeEmail(registerRequest.Email));
        if (isFound) 
        { 
            throw new ConflictException("Користувач з таким email вже існує");
        }
        UserEntitie user = UserEntitie.Create(registerRequest.FirstName, registerRequest.LastName
            , registerRequest.Email, _passwordHasher.HashPassword(registerRequest.Password));
        await _userRepository.AddUserAsync(user);
        

    }

    public async Task<string> AuthAsync(AuthRequest authRequest)
    {
        if (string.IsNullOrWhiteSpace(authRequest.Email) 
            || string.IsNullOrWhiteSpace(authRequest.Password))
        {
            throw new InvalidCredentialsException("Невірний email або пароль");
        }
        var user = await _userRepository.GetUserByEmailAsync(UserEntitie.NormalizeEmail(authRequest.Email));

        if (user is null || !_passwordHasher.PasswordVerification(authRequest.Password,user.PasswordHash))
        {
            throw new InvalidCredentialsException("Невірний email або пароль");
        }
        var token = _jwtProvider.GenerateJWT(user);
        return token;
    }
}
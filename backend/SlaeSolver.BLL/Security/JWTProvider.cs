using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SlaeSolver.Domain.Entities;

namespace SlaeSolver.BLL.Security;

public class JWTProvider : IJWTProvider
{
    private readonly JWTOptions _jwtOptions;

    public JWTProvider(IOptions<JWTOptions> options)
    {
        _jwtOptions = options.Value;
    }

    public string GenerateJWT(UserEntitie user)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("UserFirstName", user.FirstName),
            new("UserLastName", user.LastName),
            new(ClaimTypes.Email, user.Email)
        ];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)), SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpireHours)
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }
    
    
}
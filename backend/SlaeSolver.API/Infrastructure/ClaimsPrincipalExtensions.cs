using System.Security.Claims;
using SlaeSolver.Domain.Exceptions;

namespace SlaeSolver.API.Infrastructure;

public static  class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out Guid userGuidId))
        {
            throw new InvalidCredentialsException("Невірний токен");
        }

        return userGuidId;
    }
}
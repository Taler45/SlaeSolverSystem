using SlaeSolver.Domain.Entities;

namespace SlaeSolver.BLL.Security;

public interface IJWTProvider
{
    string GenerateJWT(UserEntitie user);
}
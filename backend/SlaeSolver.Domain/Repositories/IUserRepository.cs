using SlaeSolver.Domain.Entities;

namespace SlaeSolver.Domain.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync (string email);
    Task<UserEntitie?> GetUserByEmailAsync(string email);
    Task<UserEntitie?> GetUserByIdAsync(Guid id);
    Task AddUserAsync(UserEntitie user);
    

}
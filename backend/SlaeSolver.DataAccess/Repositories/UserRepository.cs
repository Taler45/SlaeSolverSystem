using Microsoft.EntityFrameworkCore;
using SlaeSolver.DataAccess.Persistence;
using SlaeSolver.Domain.Entities;
using SlaeSolver.Domain.Repositories;

namespace SlaeSolver.DataAccess.Repositories;

public class UserRepository: IUserRepository
{

    private readonly SlaeSolverContext _context;

    public UserRepository(SlaeSolverContext context)
    {
        _context = context;
    }
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AsNoTracking().AnyAsync(u => u.Email ==email);
    }

    public async Task<UserEntitie?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        
        if (user is null)
        {
            return null;
        }

        return user;
    }

    public async Task<UserEntitie?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return null;
        }

        return user;
    }

    public async Task AddUserAsync(UserEntitie user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
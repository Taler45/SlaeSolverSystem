using Microsoft.EntityFrameworkCore;
using SlaeSolver.DataAccess.Persistence;
using SlaeSolver.Domain;
using SlaeSolver.Domain.Entities;
using SlaeSolver.Domain.Repositories;

namespace SlaeSolver.DataAccess.Repositories;

public class AdminRepository: IAdminRepository
{
    private readonly SlaeSolverContext _context;

    public AdminRepository(SlaeSolverContext context)
    {
        _context = context;
    }


    public async Task<List<TaskEntitie>> GetActiveWithUsersAsync(CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Status == CalculationStatus.InProgress || t.Status == CalculationStatus.Queued)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(CalculationStatus status, DateTime? since = null, CancellationToken ct = default)
    {
        
        if (since == null)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.Status == status)
                .CountAsync(ct);
           
        }
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Status == status && t.CompletedAt  >= since)
            .CountAsync(ct);
    }
}
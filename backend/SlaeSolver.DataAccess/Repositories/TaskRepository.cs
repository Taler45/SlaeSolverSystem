using Microsoft.EntityFrameworkCore;
using SlaeSolver.DataAccess.Persistence;
using SlaeSolver.Domain;
using SlaeSolver.Domain.Entities;
using SlaeSolver.Domain.Repositories;

namespace SlaeSolver.DataAccess.Repositories;

public class TaskRepository: ITaskRepository
{
    private readonly SlaeSolverContext _context;

    public TaskRepository(SlaeSolverContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(TaskEntitie task)
    {
        await _context.Tasks.AddAsync(task);
    }

    public async Task<TaskEntitie?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskEntitie?> GetByIdForUserAsync(Guid id, Guid userId)
    {
        return await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<IEnumerable<TaskEntitie>> GetByUserAsync(Guid userId, int page, int pageSize)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountActiveByUserAsync(Guid userId)
    {
        return await _context.Tasks
            .CountAsync(t => t.UserId == userId 
                             && (t.Status == CalculationStatus.Queued || t.Status == CalculationStatus.InProgress));
    }

    public async Task<bool> IsCancelRequestedAsync(Guid id)
    {
        return await _context.Tasks
            .Where(t => t.Id == id)
            .Select(t => t.IsCanceled)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid?> ClaimNextQueuedAsync(string serverId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var candidates = await _context.Tasks
            .FromSql($"""
                      SELECT * FROM "Tasks"
                      WHERE "Status" = 'Queued'
                      ORDER BY "CreatedAt"
                      LIMIT 1
                      FOR UPDATE SKIP LOCKED
                      """)
            .ToListAsync();

        var task = candidates.FirstOrDefault();

        if (task is null)
        {
            await transaction.CommitAsync();
            return null;
        }

        task.Start(serverId);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return task.Id;
    }

    public async Task UpdateProgressAsync(Guid id, int progress)
    {
        await _context.Tasks
            .Where(t => t.Id == id
                        && t.Status == CalculationStatus.InProgress
                        && t.Progress < progress)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Progress, progress));
    }

    public async Task<IEnumerable<TaskEntitie>> GetInProgressByServerAsync(string serverId)
    {
        return await _context.Tasks.Where(t => t.Status == CalculationStatus.InProgress && t.ServerId == serverId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
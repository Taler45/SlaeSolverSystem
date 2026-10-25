using SlaeSolver.Domain.Entities;

namespace SlaeSolver.Domain.Repositories;

public interface ITaskRepository
{
    Task AddAsync(TaskEntitie task);
    Task<TaskEntitie?> GetByIdAsync(Guid id);
    Task<TaskEntitie?> GetByIdForUserAsync(Guid id, Guid userId);
    Task<IEnumerable<TaskEntitie>> GetByUserAsync(Guid userId, int page, int pageSize);
    Task<int> CountActiveByUserAsync(Guid userId);
    Task<bool> IsCancelRequestedAsync(Guid id);
    Task SaveChangesAsync ();
}
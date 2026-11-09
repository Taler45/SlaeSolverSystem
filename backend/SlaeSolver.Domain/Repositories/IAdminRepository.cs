using SlaeSolver.Domain.Entities;

namespace SlaeSolver.Domain.Repositories;

public interface IAdminRepository
{
    Task<List<TaskEntitie>> GetActiveWithUsersAsync(CancellationToken ct = default);
    Task<int> CountAsync (CalculationStatus status, DateTime? since = null, CancellationToken ct = default);
}
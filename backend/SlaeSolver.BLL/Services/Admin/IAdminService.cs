using SlaeSolver.BLL.DTOs.Admin;

namespace SlaeSolver.BLL.Services.Admin;

public interface IAdminService
{
    Task<AdminOverviewDto> GetOverviewAsync(CancellationToken ct = default);
}
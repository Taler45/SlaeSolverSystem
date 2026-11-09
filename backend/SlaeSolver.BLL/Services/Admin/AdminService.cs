using SlaeSolver.BLL.DTOs.Admin;
using SlaeSolver.Domain;
using SlaeSolver.Domain.Repositories;

namespace SlaeSolver.BLL.Services.Admin;

public class AdminService: IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<AdminOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddHours(-1);

        var queued = await _adminRepository.CountAsync(CalculationStatus.Queued, ct: ct);
        var inProgress = await _adminRepository.CountAsync(CalculationStatus.InProgress, ct: ct);
        var doneLastHour = await _adminRepository.CountAsync(CalculationStatus.Done, since, ct);
        var failedLastHour = await _adminRepository.CountAsync(CalculationStatus.Error, since, ct);
        var doneTotal = await _adminRepository.CountAsync(CalculationStatus.Done, ct: ct);
        var failedTotal = await _adminRepository.CountAsync(CalculationStatus.Error, ct: ct);
        var canceledTotal = await _adminRepository.CountAsync(CalculationStatus.Canceled, ct: ct);

        var stats = new AdminStatsDto(
            queued, inProgress, doneLastHour, failedLastHour,
            doneTotal, failedTotal, canceledTotal);

        var activeTasks = await _adminRepository.GetActiveWithUsersAsync(ct);

        var adminTasks = new List<AdminTaskDto>();

        foreach (var t in activeTasks)
        {
            adminTasks.Add(new AdminTaskDto(
                t.Id, t.User.Email, t.MatrixSize, t.Status.ToString(), t.Progress, t.CreatedAt));
        }

        return new AdminOverviewDto(stats, adminTasks);
    }
}
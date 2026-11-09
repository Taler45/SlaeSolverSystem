namespace SlaeSolver.BLL.DTOs.Admin;

public record AdminOverviewDto(
    AdminStatsDto Stats,
    IReadOnlyList<AdminTaskDto> ActiveTasks
    );
namespace SlaeSolver.BLL.DTOs.Admin;

public record AdminStatsDto(
    int Queued,
    int InProgress,
    int DoneLastHour,
    int FailedLastHour,
    int DoneTotal,
    int FailedTotal,
    int CanceledTotal
    );
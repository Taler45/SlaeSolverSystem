namespace SlaeSolver.BLL.DTOs.Internal;

public record ServerLoadDto(
    string ServerId,
    int ActiveTasks, 
    int MaxTasks, 
    int FreeSlots, 
    long TotalWeight
    );

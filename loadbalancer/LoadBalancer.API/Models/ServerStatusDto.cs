namespace LoadBalancer.API.Models;

public record ServerStatusDto(
    string ServerId, 
    bool IsAlive,
    int ActiveTasks, 
    int MaxTasks,
    int FreeSlots,
    long TotalWeight,
    DateTime? LastSeenAt
    );
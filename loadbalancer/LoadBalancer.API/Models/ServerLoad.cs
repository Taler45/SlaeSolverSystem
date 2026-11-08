namespace LoadBalancer.API.Models;

public record  ServerLoad(
    string ServerId, 
    int ActiveTasks, 
    int MaxTasks,
    int FreeSlots,
    long TotalWeight
    );
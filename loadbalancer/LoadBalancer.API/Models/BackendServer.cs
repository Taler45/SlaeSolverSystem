namespace LoadBalancer.API.Models;

public class BackendServer
{
    public string Id { get; }
    public string Url { get; } 
    public bool IsAlive { get; set; } = false;
    public DateTime? LastSeenAt { get; set; } = null;
    public int ActiveTasks { get; set; }
    public int MaxTasks { get; set; }
    public int FreeSlots { get; set; }
    public long TotalWeight { get; set; }
    
    public BackendServer(string id, string url)
    {
        Id = id;
        Url = url;
    }
}
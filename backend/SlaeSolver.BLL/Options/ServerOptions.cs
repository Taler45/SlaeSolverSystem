namespace SlaeSolver.BLL.Options;

public class ServerOptions
{
    public string ServerId { get; set; } = Environment.MachineName;
    public int MaxConcurrentTasks { get; set; } = 2;
    public int PollIntervalSeconds { get; set; } = 2;
    public int ProgressSaveSeconds { get; set; } = 2;
    public string InternalApiKey { get; set; } = string.Empty;
}
namespace LoadBalancer.API.Options;

public class LoadBalancerOptions
{
    public int PollIntervalSeconds { get; set; } = 2;
    public int PollTimeoutSeconds { get; set; } = 2;
    public string InternalApiKey { get; set; } = string.Empty;
    public List<ServerConfig> Servers { get; set; } = new();
}
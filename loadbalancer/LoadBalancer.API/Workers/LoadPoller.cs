using LoadBalancer.API.Models;
using LoadBalancer.API.Options;
using LoadBalancer.API.Services;
using Microsoft.Extensions.Options;

namespace LoadBalancer.API.Workers;

public class LoadPoller : BackgroundService
{
    private readonly ServerRegistry _registry;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LoadBalancerOptions _options;
    private readonly ILogger<LoadPoller> _logger;

    public LoadPoller(ServerRegistry registry, IHttpClientFactory httpClientFactory,
        IOptions<LoadBalancerOptions> options, ILogger<LoadPoller> logger)
    {
        _registry = registry;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        try
        {
            do
            {
                await PollAllAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task PollAllAsync(CancellationToken ct)
    {
        var targets = _registry.GetTargets();
        await Task.WhenAll(targets.Select(t => PollOneAsync(t.Id, t.Url, ct)));
    }

    private async Task PollOneAsync(string id, string url, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("backend");

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/internal/load");
            request.Headers.Add("X-Internal-Key", _options.InternalApiKey);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.PollTimeoutSeconds));

            using var response = await client.SendAsync(request, timeout.Token);
            response.EnsureSuccessStatusCode();

            var load = await response.Content.ReadFromJsonAsync<ServerLoad>(timeout.Token);
            if (load is not null && _registry.UpdateLoad(id, load))
            {
                _logger.LogInformation("Сервер {Id} доступний", id);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (_registry.MarkDead(id))
            {
                _logger.LogWarning("Сервер {Id} недоступний: {Message}", id, ex.Message);
            }
        }
    }
}


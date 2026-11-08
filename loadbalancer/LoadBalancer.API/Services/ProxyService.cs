using System.Text.Json;
using LoadBalancer.API.Models;

namespace LoadBalancer.API.Services;

public class ProxyService
{
     private static readonly HashSet<string> SkippedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Host", "Content-Length", "Transfer-Encoding", "Connection"
    };

    private readonly ServerRegistry _registry;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProxyService> _logger;

    public ProxyService(ServerRegistry registry, IHttpClientFactory httpClientFactory, ILogger<ProxyService> logger)
    {
        _registry = registry;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var body = await ReadBodyAsync(context.Request);

        bool isCreate = HttpMethods.IsPost(context.Request.Method)
                        && context.Request.Path.Equals("/api/tasks/create/task", StringComparison.OrdinalIgnoreCase);
        bool isGet = HttpMethods.IsGet(context.Request.Method);

        var server = isCreate
            ? _registry.ChooseForNewTask(ReadMatrixSize(body))
            : _registry.ChooseAny();

        if (server is null)
        {
            await WriteErrorAsync(context, StatusCodes.Status503ServiceUnavailable, "Немає доступних серверів");
            return;
        }

        try
        {
            await ForwardAsync(context, server, body);
        }
        catch (HttpRequestException ex)
        {
            if (_registry.MarkDead(server.Id))
            {
                _logger.LogWarning("Сервер {Id} не відповів на запит: {Message}", server.Id, ex.Message);
            }

            if (isGet && await TryAnotherAsync(context, server.Id, body))
            {
                return;
            }

            await WriteErrorAsync(context, StatusCodes.Status502BadGateway, "Сервер недоступний, спробуйте ще раз");
        }
        catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            await WriteErrorAsync(context, StatusCodes.Status504GatewayTimeout, "Сервер не відповів вчасно");
        }
    }

    private async Task<bool> TryAnotherAsync(HttpContext context, string failedId, byte[] body)
    {
        var retry = _registry.ChooseAny(failedId);
        if (retry is null)
        {
            return false;
        }

        try
        {
            await ForwardAsync(context, retry, body);
            return true;
        }
        catch (HttpRequestException)
        {
            _registry.MarkDead(retry.Id);
            return false;
        }
    }

    private async Task ForwardAsync(HttpContext context, BackendServer server, byte[] body)
    {
        var target = server.Url + context.Request.Path + context.Request.QueryString;
        using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), target);

        if (body.Length > 0)
        {
            request.Content = new ByteArrayContent(body);
        }

        foreach (var header in context.Request.Headers)
        {
            if (SkippedHeaders.Contains(header.Key))
            {
                continue;
            }

            var values = header.Value.ToArray();
            if (!request.Headers.TryAddWithoutValidation(header.Key, values))
            {
                request.Content?.Headers.TryAddWithoutValidation(header.Key, values);
            }
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        if (clientIp is not null)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-For", clientIp);
        }

        var client = _httpClientFactory.CreateClient("backend");
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted);

        context.Response.StatusCode = (int)response.StatusCode;
        foreach (var header in response.Headers.Concat(response.Content.Headers))
        {
            if (!SkippedHeaders.Contains(header.Key))
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
        }
        context.Response.Headers["X-Served-By"] = server.Id;

        await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
    }

    private static async Task<byte[]> ReadBodyAsync(HttpRequest request)
    {
        using var memory = new MemoryStream();
        await request.Body.CopyToAsync(memory);
        return memory.ToArray();
    }

    private static int ReadMatrixSize(byte[] body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Name.Equals("matrixSize", StringComparison.OrdinalIgnoreCase)
                    && property.Value.TryGetInt32(out var size))
                {
                    return size;
                }
            }
        }
        catch (JsonException)
        {
        }

        return 0;
    }

    private static async Task WriteErrorAsync(HttpContext context, int status, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { status, title = message });
    }
}
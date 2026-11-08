
using LoadBalancer.API.Options;
using LoadBalancer.API.Services;
using LoadBalancer.API.Workers;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(builder.Configuration["LoadBalancer:InternalApiKey"]))
{
    throw new InvalidOperationException("Не задано LoadBalancer:InternalApiKey");
}

builder.Services.Configure<LoadBalancerOptions>(builder.Configuration.GetSection("LoadBalancer"));

builder.Services.AddHttpClient("backend", client => client.Timeout = TimeSpan.FromSeconds(30))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false
    });

builder.Services.AddSingleton<ServerRegistry>();
builder.Services.AddSingleton<ProxyService>();
builder.Services.AddHostedService<LoadPoller>();

var app = builder.Build();

app.MapGet("/lb/servers", (ServerRegistry registry) => registry.Snapshot());
app.Map("/api/{**path}", (HttpContext context, ProxyService proxy) => proxy.HandleAsync(context)); 

app.Run();

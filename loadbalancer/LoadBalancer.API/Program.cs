using System.Text;
using LoadBalancer.API.Options;
using LoadBalancer.API.Services;
using LoadBalancer.API.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

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


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        ValidateAudience = false,
        ValidateIssuer = false
    });
builder.Services.AddAuthorization();


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var frontendPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "../../frontend"));

var frontendFiles = new PhysicalFileProvider(frontendPath);

app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = frontendFiles });
app.UseStaticFiles(new StaticFileOptions { FileProvider = frontendFiles });

app.MapGet("/lb/servers", (ServerRegistry registry) => registry.Snapshot()).RequireAuthorization(policy => policy.RequireRole("Admin"));
app.Map("/api/{**path}", (HttpContext context, ProxyService proxy) => proxy.HandleAsync(context)); 

app.Run();

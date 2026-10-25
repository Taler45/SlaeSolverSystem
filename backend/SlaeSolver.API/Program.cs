using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SlaeSolver.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SlaeSolver.API.Infrastructure;
using SlaeSolver.BLL.Options;
using SlaeSolver.BLL.Security;
using SlaeSolver.BLL.Services.Auth;
using SlaeSolver.BLL.Storage;
using SlaeSolver.DataAccess.Repositories;
using SlaeSolver.Domain.Repositories;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<SlaeSolverContext>(
    option => option.UseNpgsql(configuration.GetConnectionString(nameof(SlaeSolverContext)))
);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<IJWTProvider, JWTProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();


builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<LimitsOptions>(builder.Configuration.GetSection("Limits"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));



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
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
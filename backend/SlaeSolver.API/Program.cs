using SlaeSolver.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddControllers();



builder.Services.AddDbContext<SlaeSolverContext>(
    option => option.UseNpgsql(configuration.GetConnectionString(nameof(SlaeSolverContext)))
);
    
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
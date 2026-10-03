using Microsoft.EntityFrameworkCore;
using SlaeSolver.DataAccess.Configurations;
using SlaeSolver.Domain.Entities;

namespace SlaeSolver.DataAccess.Persistence;

public class SlaeSolverContext : DbContext
{
    public DbSet<UserEntitie> Users { get; set; }
    public DbSet<TaskEntitie> Tasks { get; set; }

    public SlaeSolverContext(DbContextOptions<SlaeSolverContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TaskEntitieConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntitieConfiguration());
    }
}

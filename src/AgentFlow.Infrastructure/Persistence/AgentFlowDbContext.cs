using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence;

public sealed class AgentFlowDbContext(DbContextOptions<AgentFlowDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("workflow");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentFlowDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

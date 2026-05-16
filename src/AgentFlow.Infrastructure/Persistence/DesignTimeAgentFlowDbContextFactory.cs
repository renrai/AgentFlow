using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgentFlow.Infrastructure.Persistence;

public sealed class DesignTimeAgentFlowDbContextFactory : IDesignTimeDbContextFactory<AgentFlowDbContext>
{
    public AgentFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? "Host=localhost;Port=5432;Database=agentflow;Username=agentflow;Password=agentflow";

        var options = new DbContextOptionsBuilder<AgentFlowDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AgentFlowDbContext).Assembly.FullName))
            .Options;

        return new AgentFlowDbContext(options);
    }
}

using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Tenants;
using AgentFlow.Domain.Users;
using AgentFlow.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence;

public sealed class AgentFlowDbContext(DbContextOptions<AgentFlowDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantMember> TenantMembers => Set<TenantMember>();

    public DbSet<Workflow> Workflows => Set<Workflow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("workflow");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentFlowDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

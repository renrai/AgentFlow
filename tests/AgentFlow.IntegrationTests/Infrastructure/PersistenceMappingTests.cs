using AgentFlow.Domain.Tenants;
using AgentFlow.Domain.Users;
using AgentFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.IntegrationTests.Infrastructure;

public sealed class PersistenceMappingTests
{
    [Fact]
    public void IdentityEntitiesShouldMapToIdentitySchema()
    {
        var options = new DbContextOptionsBuilder<AgentFlowDbContext>()
            .UseNpgsql("Host=localhost;Database=agentflow;Username=agentflow;Password=agentflow")
            .Options;

        using var dbContext = new AgentFlowDbContext(options);

        Assert.Equal("identity", dbContext.Model.FindEntityType(typeof(User))?.GetSchema());
        Assert.Equal("users", dbContext.Model.FindEntityType(typeof(User))?.GetTableName());
        Assert.Equal("identity", dbContext.Model.FindEntityType(typeof(Tenant))?.GetSchema());
        Assert.Equal("tenants", dbContext.Model.FindEntityType(typeof(Tenant))?.GetTableName());
        Assert.Equal("identity", dbContext.Model.FindEntityType(typeof(TenantMember))?.GetSchema());
        Assert.Equal("tenant_members", dbContext.Model.FindEntityType(typeof(TenantMember))?.GetTableName());
    }
}

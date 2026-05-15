namespace AgentFlow.IntegrationTests.Infrastructure;

public sealed class BootstrapTests
{
    [Fact]
    public void InfrastructureProjectShouldBeLoadable()
    {
        var assemblyName = typeof(AgentFlow.Infrastructure.DependencyInjection)
            .Assembly
            .GetName()
            .Name;

        Assert.Equal("AgentFlow.Infrastructure", assemblyName);
    }
}

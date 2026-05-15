using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.UnitTests.Architecture;

public sealed class DependencyDirectionTests
{
    [Fact]
    public void DomainProjectShouldNotReferenceOuterLayers()
    {
        var referencedAssemblies = typeof(AggregateRoot<>)
            .Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain("AgentFlow.Application", referencedAssemblies);
        Assert.DoesNotContain("AgentFlow.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("AgentFlow.Api", referencedAssemblies);
        Assert.DoesNotContain("AgentFlow.Worker", referencedAssemblies);
    }
}

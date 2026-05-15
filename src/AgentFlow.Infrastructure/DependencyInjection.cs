using AgentFlow.Infrastructure.Caching;
using AgentFlow.Infrastructure.Messaging;
using AgentFlow.Infrastructure.Observability;
using AgentFlow.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        bool includeAspNetCoreInstrumentation = true)
    {
        services.AddPersistence(configuration);
        services.AddRedis(configuration);
        services.AddRabbitMq(configuration);
        services.AddPlatformOpenTelemetry(configuration, serviceName, includeAspNetCoreInstrumentation);

        return services;
    }
}

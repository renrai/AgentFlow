using AgentFlow.Infrastructure.Caching;
using AgentFlow.Infrastructure.Identity;
using AgentFlow.Infrastructure.Messaging;
using AgentFlow.Infrastructure.Observability;
using AgentFlow.Infrastructure.Persistence;
using AgentFlow.Infrastructure.Time;
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
        services.AddPlatformClock();
        services.AddPersistence(configuration);
        services.AddRedis(configuration);
        services.AddRabbitMq(configuration);
        services.AddPlatformIdentity(configuration);
        services.AddPlatformOpenTelemetry(configuration, serviceName, includeAspNetCoreInstrumentation);

        return services;
    }
}

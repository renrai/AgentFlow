using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Infrastructure.Caching;

public static class DependencyInjection
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' is required.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "agentflow:";
        });

        return services;
    }
}

using AgentFlow.Application.Abstractions.Clock;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Infrastructure.Time;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformClock(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}

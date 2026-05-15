using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}

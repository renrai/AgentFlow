using AgentFlow.Application.Abstractions.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
        services.AddHostedService<RabbitMqTopologyInitializer>();

        return services;
    }
}

using AgentFlow.Application.Abstractions.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Infrastructure.AiProviders;

public static class DependencyInjection
{
    public static IServiceCollection AddAiProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiProviderOptions>()
            .Bind(configuration.GetSection(AiProviderOptions.SectionName));

        services.AddHttpClient(AnthropicChatClient.HttpClientName);
        services.AddHttpClient(OpenAiChatClient.HttpClientName);

        services.AddSingleton<IAiChatClient, AnthropicChatClient>();
        services.AddSingleton<IAiChatClient, OpenAiChatClient>();
        services.AddSingleton<IAiChatClient, MockChatClient>();
        services.AddSingleton<IAiChatClientRegistry, AiChatClientRegistry>();

        return services;
    }
}

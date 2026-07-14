using AgentFlow.Application.Abstractions.Ai;

namespace AgentFlow.Infrastructure.AiProviders;

internal sealed class AiChatClientRegistry : IAiChatClientRegistry
{
    private readonly Dictionary<string, IAiChatClient> _clients;

    public AiChatClientRegistry(IEnumerable<IAiChatClient> clients)
    {
        _clients = clients.ToDictionary(c => c.Provider, StringComparer.OrdinalIgnoreCase);
    }

    public IAiChatClient Resolve(string provider)
    {
        if (_clients.TryGetValue(provider, out var client))
            return client;

        throw new InvalidOperationException(
            $"Unknown AI provider '{provider}'. Available providers: {string.Join(", ", _clients.Keys)}.");
    }
}

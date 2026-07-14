using AgentFlow.Application.Abstractions.Ai;

namespace AgentFlow.Infrastructure.AiProviders;

/// <summary>
/// Deterministic offline provider so workflows with AI nodes can be
/// demonstrated and tested without real API keys.
/// </summary>
internal sealed class MockChatClient : IAiChatClient
{
    public string Provider => "mock";

    public Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        var content = $"[mock:{request.Model}] {request.UserPrompt}";

        return Task.FromResult(new AiChatResult(
            Provider,
            request.Model,
            content,
            InputTokens: request.UserPrompt.Length / 4,
            OutputTokens: content.Length / 4));
    }
}

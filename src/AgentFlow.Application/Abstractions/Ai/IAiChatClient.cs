namespace AgentFlow.Application.Abstractions.Ai;

public interface IAiChatClient
{
    string Provider { get; }

    Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default);
}

public sealed record AiChatRequest(
    string Model,
    string? SystemPrompt,
    string UserPrompt,
    int MaxTokens);

public sealed record AiChatResult(
    string Provider,
    string Model,
    string Content,
    int InputTokens,
    int OutputTokens);

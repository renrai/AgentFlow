using System.Text.Json;
using AgentFlow.Application.Abstractions.Ai;
using AgentFlow.Worker.Execution;
using Microsoft.Extensions.Logging;

namespace AgentFlow.Worker.Execution.Nodes;

internal sealed class AiChatNodeExecutor(
    IAiChatClientRegistry registry,
    ILogger<AiChatNodeExecutor> logger) : INodeExecutor
{
    private const string InputPlaceholder = "{{input}}";
    private const int DefaultMaxTokens = 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string NodeType => "ai.chat";

    public async Task<NodeExecutionResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var config = JsonSerializer.Deserialize<AiChatNodeConfig>(context.NodeConfiguration, JsonOptions)
            ?? throw new InvalidOperationException("Invalid AI chat node configuration.");

        if (string.IsNullOrWhiteSpace(config.Provider))
            throw new InvalidOperationException("AI chat node 'provider' is required (anthropic, openai, or mock).");

        if (string.IsNullOrWhiteSpace(config.Model))
            throw new InvalidOperationException("AI chat node 'model' is required.");

        var template = string.IsNullOrWhiteSpace(config.UserPromptTemplate)
            ? InputPlaceholder
            : config.UserPromptTemplate;

        var userPrompt = template.Replace(InputPlaceholder, context.Input ?? "{}", StringComparison.Ordinal);

        var client = registry.Resolve(config.Provider);

        LogAiCall(logger, config.Provider, config.Model, null);

        var result = await client
            .CompleteAsync(
                new AiChatRequest(
                    config.Model,
                    config.SystemPrompt,
                    userPrompt,
                    config.MaxTokens is > 0 ? config.MaxTokens.Value : DefaultMaxTokens),
                cancellationToken)
            .ConfigureAwait(false);

        var output = JsonSerializer.Serialize(
            new
            {
                provider = result.Provider,
                model = result.Model,
                content = result.Content,
                usage = new
                {
                    inputTokens = result.InputTokens,
                    outputTokens = result.OutputTokens
                }
            },
            JsonOptions);

        return new NodeExecutionResult(output);
    }

    private sealed record AiChatNodeConfig(
        string? Provider,
        string? Model,
        string? SystemPrompt,
        string? UserPromptTemplate,
        int? MaxTokens);

    private static readonly Action<ILogger, string, string, Exception?> LogAiCall =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(7200, nameof(LogAiCall)),
            "AI chat node calling provider {Provider} with model {Model}");
}

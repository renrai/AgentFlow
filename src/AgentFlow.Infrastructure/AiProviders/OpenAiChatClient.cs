using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentFlow.Application.Abstractions.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentFlow.Infrastructure.AiProviders;

internal sealed class OpenAiChatClient(
    IHttpClientFactory httpClientFactory,
    IOptions<AiProviderOptions> options,
    ILogger<OpenAiChatClient> logger) : IAiChatClient
{
    public const string HttpClientName = "ai-openai";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AiProviderOptions.OpenAiOptions _options = options.Value.OpenAi;

    public string Provider => "openai";

    public async Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("OpenAI API key is not configured. Set 'AiProviders:OpenAi:ApiKey'.");

        using var client = httpClientFactory.CreateClient(HttpClientName);
        client.BaseAddress = new Uri(_options.BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var messages = new List<OpenAiMessage>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            messages.Add(new OpenAiMessage("system", request.SystemPrompt));
        messages.Add(new OpenAiMessage("user", request.UserPrompt));

        var body = new OpenAiRequest(request.Model, messages, request.MaxTokens);

        LogRequest(logger, request.Model, null);

        using var response = await client
            .PostAsJsonAsync("/v1/chat/completions", body, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"OpenAI API returned {(int)response.StatusCode}: {Truncate(responseBody, 500)}");

        var parsed = JsonSerializer.Deserialize<OpenAiResponse>(responseBody, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI API returned an empty response.");

        var content = parsed.Choices.Count > 0
            ? parsed.Choices[0].Message?.Content ?? string.Empty
            : string.Empty;

        return new AiChatResult(
            Provider,
            parsed.Model ?? request.Model,
            content,
            parsed.Usage?.PromptTokens ?? 0,
            parsed.Usage?.CompletionTokens ?? 0);
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private sealed record OpenAiRequest(
        string Model,
        IReadOnlyList<OpenAiMessage> Messages,
        [property: JsonPropertyName("max_completion_tokens")] int MaxCompletionTokens);

    private sealed record OpenAiMessage(string Role, string Content);

    private sealed record OpenAiResponse(
        string? Model,
        IReadOnlyList<OpenAiChoice> Choices,
        OpenAiUsage? Usage)
    {
        public IReadOnlyList<OpenAiChoice> Choices { get; init; } = Choices ?? [];
    }

    private sealed record OpenAiChoice(OpenAiMessage? Message);

    private sealed record OpenAiUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);

    private static readonly Action<ILogger, string, Exception?> LogRequest =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(9001, nameof(LogRequest)),
            "Calling OpenAI Chat Completions API with model {Model}");
}

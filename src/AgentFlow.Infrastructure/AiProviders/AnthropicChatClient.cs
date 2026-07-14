using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentFlow.Application.Abstractions.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentFlow.Infrastructure.AiProviders;

internal sealed class AnthropicChatClient(
    IHttpClientFactory httpClientFactory,
    IOptions<AiProviderOptions> options,
    ILogger<AnthropicChatClient> logger) : IAiChatClient
{
    public const string HttpClientName = "ai-anthropic";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AiProviderOptions.AnthropicOptions _options = options.Value.Anthropic;

    public string Provider => "anthropic";

    public async Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Anthropic API key is not configured. Set 'AiProviders:Anthropic:ApiKey'.");

        using var client = httpClientFactory.CreateClient(HttpClientName);
        client.BaseAddress = new Uri(_options.BaseUrl);
        client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", _options.ApiVersion);

        var body = new AnthropicRequest(
            request.Model,
            request.MaxTokens,
            request.SystemPrompt,
            [new AnthropicMessage("user", request.UserPrompt)]);

        LogRequest(logger, request.Model, null);

        using var response = await client
            .PostAsJsonAsync("/v1/messages", body, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Anthropic API returned {(int)response.StatusCode}: {Truncate(responseBody, 500)}");

        var parsed = JsonSerializer.Deserialize<AnthropicResponse>(responseBody, JsonOptions)
            ?? throw new InvalidOperationException("Anthropic API returned an empty response.");

        var content = string.Concat(
            parsed.Content
                .Where(block => block.Type == "text")
                .Select(block => block.Text));

        return new AiChatResult(
            Provider,
            parsed.Model ?? request.Model,
            content,
            parsed.Usage?.InputTokens ?? 0,
            parsed.Usage?.OutputTokens ?? 0);
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private sealed record AnthropicRequest(
        string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        string? System,
        IReadOnlyList<AnthropicMessage> Messages);

    private sealed record AnthropicMessage(string Role, string Content);

    private sealed record AnthropicResponse(
        string? Model,
        IReadOnlyList<AnthropicContentBlock> Content,
        AnthropicUsage? Usage)
    {
        public IReadOnlyList<AnthropicContentBlock> Content { get; init; } = Content ?? [];
    }

    private sealed record AnthropicContentBlock(string Type, string? Text);

    private sealed record AnthropicUsage(
        [property: JsonPropertyName("input_tokens")] int InputTokens,
        [property: JsonPropertyName("output_tokens")] int OutputTokens);

    private static readonly Action<ILogger, string, Exception?> LogRequest =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(9000, nameof(LogRequest)),
            "Calling Anthropic Messages API with model {Model}");
}

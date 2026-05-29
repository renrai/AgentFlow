using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AgentFlow.Worker.Execution;
using Microsoft.Extensions.Logging;

namespace AgentFlow.Worker.Execution.Nodes;

internal sealed class HttpRequestNodeExecutor(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpRequestNodeExecutor> logger) : INodeExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string NodeType => "action.http";

    public async Task<NodeExecutionResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var config = JsonSerializer.Deserialize<HttpNodeConfig>(context.NodeConfiguration, JsonOptions)
            ?? throw new InvalidOperationException("Invalid HTTP node configuration.");

        if (string.IsNullOrWhiteSpace(config.Url))
            throw new InvalidOperationException("HTTP node 'url' is required.");

        var method = string.IsNullOrWhiteSpace(config.Method) ? "GET" : config.Method.ToUpperInvariant();
        using var client = httpClientFactory.CreateClient("workflow-http");
        using var request = new HttpRequestMessage(new HttpMethod(method), config.Url);

        if (config.Headers is not null)
        {
            foreach (var (key, value) in config.Headers)
            {
                request.Headers.TryAddWithoutValidation(key, value);
            }
        }

        if (!string.IsNullOrEmpty(context.Input) && method is "POST" or "PUT" or "PATCH")
        {
            request.Content = new StringContent(context.Input, Encoding.UTF8);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        LogHttpCall(logger, method, config.Url, null);

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {Truncate(body, 500)}");

        object bodyValue = TryParseJson(body) is { } parsed ? parsed : body;
        var output = new
        {
            statusCode = (int)response.StatusCode,
            body = bodyValue
        };

        return new NodeExecutionResult(JsonSerializer.Serialize(output, JsonOptions));
    }

    private static JsonElement? TryParseJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(value, JsonOptions);
        }
        catch (JsonException) { return null; }
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private sealed record HttpNodeConfig(string? Url, string? Method, Dictionary<string, string>? Headers);

    private static readonly Action<ILogger, string, string, Exception?> LogHttpCall =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(7100, nameof(LogHttpCall)),
            "HTTP node calling {Method} {Url}");
}

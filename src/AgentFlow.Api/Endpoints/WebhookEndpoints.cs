using AgentFlow.Application.Webhooks.TriggerWebhook;

namespace AgentFlow.Api.Endpoints;

public static class WebhookEndpoints
{
    private const int MaxPayloadBytes = 1024 * 1024; // 1 MB safety cap.

    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/webhooks")
            .WithTags("Webhooks")
            .AllowAnonymous();

        group.MapPost("/{token}", async (
            string token,
            HttpRequest request,
            TriggerWebhookHandler handler,
            CancellationToken cancellationToken) =>
        {
            var payload = await ReadPayloadAsync(request, cancellationToken).ConfigureAwait(false);
            var command = new TriggerWebhookCommand(token, payload);
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.Accepted(
                $"/tenants/{result.TenantId}/executions/{result.ExecutionId}",
                result);
        })
        .WithName("TriggerWebhook")
        .WithSummary("Triggers an execution of a published workflow using its webhook token.");

        return app;
    }

    private static async Task<string?> ReadPayloadAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.ContentLength is 0 || request.ContentLength is null && !request.Body.CanRead)
            return null;

        if (request.ContentLength is { } length && length > MaxPayloadBytes)
            throw new InvalidOperationException("Webhook payload exceeds maximum allowed size.");

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(body) ? null : body;
    }
}

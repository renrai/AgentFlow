namespace AgentFlow.Application.Webhooks.TriggerWebhook;

public sealed record TriggerWebhookCommand(string Token, string? Payload);

public sealed record TriggerWebhookResult(
    Guid ExecutionId,
    Guid WorkflowId,
    Guid TenantId,
    int WorkflowVersion,
    string Status,
    DateTimeOffset CreatedAtUtc);

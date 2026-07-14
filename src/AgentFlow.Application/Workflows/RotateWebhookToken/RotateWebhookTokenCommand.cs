namespace AgentFlow.Application.Workflows.RotateWebhookToken;

public sealed record RotateWebhookTokenCommand(Guid WorkflowId, Guid TenantId, Guid UserId);

public sealed record RotateWebhookTokenResult(Guid WorkflowId, string WebhookToken);

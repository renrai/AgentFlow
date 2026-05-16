namespace AgentFlow.Application.Workflows.PublishWorkflow;

public sealed record PublishWorkflowCommand(
    Guid WorkflowId,
    Guid TenantId,
    Guid UserId);

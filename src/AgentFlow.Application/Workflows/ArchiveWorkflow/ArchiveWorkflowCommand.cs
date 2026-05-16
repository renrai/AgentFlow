namespace AgentFlow.Application.Workflows.ArchiveWorkflow;

public sealed record ArchiveWorkflowCommand(
    Guid WorkflowId,
    Guid TenantId,
    Guid UserId);

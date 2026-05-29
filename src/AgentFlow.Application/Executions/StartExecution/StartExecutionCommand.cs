namespace AgentFlow.Application.Executions.StartExecution;

public sealed record StartExecutionCommand(
    Guid WorkflowId,
    Guid TenantId,
    Guid UserId,
    string? TriggerPayload);

public sealed record StartExecutionResult(
    Guid ExecutionId,
    Guid WorkflowId,
    int WorkflowVersion,
    string Status,
    DateTimeOffset CreatedAtUtc);

namespace AgentFlow.Application.Executions.GetExecution;

public sealed record GetExecutionQuery(Guid ExecutionId, Guid TenantId, Guid UserId);

public sealed record ExecutionResult(
    Guid ExecutionId,
    Guid WorkflowId,
    int WorkflowVersion,
    string Status,
    string? TriggerPayload,
    string? ErrorMessage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<StepResult> Steps);

public sealed record StepResult(
    Guid StepId,
    Guid NodeId,
    string NodeType,
    string NodeName,
    int Sequence,
    string Status,
    string? Input,
    string? Output,
    string? ErrorMessage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc);

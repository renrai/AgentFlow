namespace AgentFlow.Worker.Execution;

public sealed record NodeExecutionContext(
    Guid ExecutionId,
    Guid TenantId,
    Guid WorkflowId,
    Guid NodeId,
    string NodeType,
    string NodeName,
    string NodeConfiguration,
    string? Input);

public sealed record NodeExecutionResult(string? Output);

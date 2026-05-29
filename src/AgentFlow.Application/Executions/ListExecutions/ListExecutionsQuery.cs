namespace AgentFlow.Application.Executions.ListExecutions;

public sealed record ListExecutionsQuery(
    Guid WorkflowId,
    Guid TenantId,
    Guid UserId,
    int Limit = 50);

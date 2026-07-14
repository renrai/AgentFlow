using AgentFlow.Domain.Executions;

namespace AgentFlow.Application.Executions.ListExecutions;

public sealed record ListExecutionsQuery(
    Guid TenantId,
    Guid UserId,
    Guid? WorkflowId = null,
    ExecutionStatus? Status = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int PageSize = 20);

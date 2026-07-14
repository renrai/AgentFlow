using AgentFlow.Application.SharedKernel;
using AgentFlow.Domain.Executions;

namespace AgentFlow.Application.Abstractions.Persistence;

public interface IExecutionRepository
{
    void Add(WorkflowExecution execution);

    Task<WorkflowExecution?> GetByIdAsync(Guid executionId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<WorkflowExecution?> GetByIdAsync(Guid executionId, CancellationToken cancellationToken = default);

    Task<PagedResult<ExecutionSummary>> SearchAsync(
        ExecutionSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}

public sealed record ExecutionSearchCriteria(
    Guid TenantId,
    Guid? WorkflowId,
    ExecutionStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize);

public sealed record ExecutionSummary(
    Guid ExecutionId,
    Guid WorkflowId,
    int WorkflowVersion,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc);

using AgentFlow.Domain.Executions;

namespace AgentFlow.Application.Abstractions.Persistence;

public interface IExecutionRepository
{
    void Add(WorkflowExecution execution);

    Task<WorkflowExecution?> GetByIdAsync(Guid executionId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<WorkflowExecution?> GetByIdAsync(Guid executionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExecutionSummary>> ListByWorkflowAsync(
        Guid workflowId,
        Guid tenantId,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed record ExecutionSummary(
    Guid ExecutionId,
    Guid WorkflowId,
    int WorkflowVersion,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc);

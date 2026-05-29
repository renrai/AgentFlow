using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Executions;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence.Repositories;

internal sealed class ExecutionRepository(AgentFlowDbContext dbContext) : IExecutionRepository
{
    public void Add(WorkflowExecution execution) => dbContext.WorkflowExecutions.Add(execution);

    public Task<WorkflowExecution?> GetByIdAsync(
        Guid executionId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => dbContext.WorkflowExecutions
            .Include(e => e.Steps)
            .FirstOrDefaultAsync(e => e.Id == executionId && e.TenantId == tenantId, cancellationToken);

    public Task<WorkflowExecution?> GetByIdAsync(Guid executionId, CancellationToken cancellationToken = default)
        => dbContext.WorkflowExecutions
            .Include(e => e.Steps)
            .FirstOrDefaultAsync(e => e.Id == executionId, cancellationToken);

    public async Task<IReadOnlyList<ExecutionSummary>> ListByWorkflowAsync(
        Guid workflowId,
        Guid tenantId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkflowExecutions
            .AsNoTracking()
            .Where(e => e.WorkflowId == workflowId && e.TenantId == tenantId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(limit)
            .Select(e => new ExecutionSummary(
                e.Id,
                e.WorkflowId,
                e.WorkflowVersion,
                e.Status.ToString(),
                e.CreatedAtUtc,
                e.StartedAtUtc,
                e.CompletedAtUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

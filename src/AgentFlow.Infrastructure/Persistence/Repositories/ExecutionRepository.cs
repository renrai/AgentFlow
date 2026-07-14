using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;
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

    public async Task<PagedResult<ExecutionSummary>> SearchAsync(
        ExecutionSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.WorkflowExecutions
            .AsNoTracking()
            .Where(e => e.TenantId == criteria.TenantId);

        if (criteria.WorkflowId is { } workflowId)
            query = query.Where(e => e.WorkflowId == workflowId);

        if (criteria.Status is { } status)
            query = query.Where(e => e.Status == status);

        if (criteria.From is { } from)
            query = query.Where(e => e.CreatedAtUtc >= from);

        if (criteria.To is { } to)
            query = query.Where(e => e.CreatedAtUtc <= to);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
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

        return new PagedResult<ExecutionSummary>(items, totalCount, criteria.Page, criteria.PageSize);
    }
}

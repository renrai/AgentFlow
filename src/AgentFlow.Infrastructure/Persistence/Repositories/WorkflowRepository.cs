using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence.Repositories;

internal sealed class WorkflowRepository(AgentFlowDbContext dbContext) : IWorkflowRepository
{
    public void Add(Workflow workflow) => dbContext.Workflows.Add(workflow);

    public Task<Workflow?> GetByIdAsync(Guid workflowId, Guid tenantId, CancellationToken cancellationToken = default)
        => dbContext.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Edges)
            .FirstOrDefaultAsync(w => w.Id == workflowId && w.TenantId == tenantId, cancellationToken);

    public Task<Workflow?> GetByWebhookTokenAsync(string token, CancellationToken cancellationToken = default)
        => dbContext.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Edges)
            .FirstOrDefaultAsync(w => w.WebhookToken == token, cancellationToken);

    public async Task<IReadOnlyList<WorkflowSummary>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Workflows
            .AsNoTracking()
            .Where(w => w.TenantId == tenantId)
            .OrderByDescending(w => w.UpdatedAtUtc)
            .Select(w => new WorkflowSummary(
                w.Id,
                w.Name,
                w.Description,
                w.Status.ToString(),
                w.Version,
                w.UpdatedAtUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

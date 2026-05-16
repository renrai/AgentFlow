using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Application.Workflows.ListWorkflows;

public sealed class ListWorkflowsHandler(
    IWorkflowRepository workflows,
    ITenantRepository tenants)
{
    public async Task<IReadOnlyList<WorkflowSummary>> HandleAsync(
        ListWorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        var isMember = await tenants
            .IsMemberAsync(query.TenantId, query.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this tenant.");

        var summaries = await workflows
            .ListByTenantAsync(query.TenantId, cancellationToken)
            .ConfigureAwait(false);

        return summaries
            .Select(s => new WorkflowSummary(s.WorkflowId, s.Name, s.Description, s.Status, s.Version, s.UpdatedAtUtc))
            .ToList();
    }
}

public sealed record WorkflowSummary(
    Guid WorkflowId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset UpdatedAtUtc);

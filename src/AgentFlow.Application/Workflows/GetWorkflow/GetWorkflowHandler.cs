using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Application.Workflows.GetWorkflow;

public sealed class GetWorkflowHandler(
    IWorkflowRepository workflows,
    ITenantRepository tenants)
{
    public async Task<WorkflowResult> HandleAsync(GetWorkflowQuery query, CancellationToken cancellationToken)
    {
        var isMember = await tenants
            .IsMemberAsync(query.TenantId, query.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this tenant.");

        var workflow = await workflows
            .GetByIdAsync(query.WorkflowId, query.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (workflow is null)
            throw new NotFoundException($"Workflow '{query.WorkflowId}' not found.");

        return new WorkflowResult(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Status.ToString(),
            workflow.Version,
            workflow.WebhookToken,
            workflow.CreatedAtUtc,
            workflow.UpdatedAtUtc,
            workflow.Nodes.Select(n => new NodeResult(n.Id, n.Type, n.Name, n.PositionX, n.PositionY, n.Configuration)).ToList(),
            workflow.Edges.Select(e => new EdgeResult(e.Id, e.SourceNodeId, e.TargetNodeId, e.Label)).ToList());
    }
}

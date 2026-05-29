using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Application.Executions.ListExecutions;

public sealed class ListExecutionsHandler(
    IExecutionRepository executions,
    ITenantRepository tenants)
{
    public async Task<IReadOnlyList<ExecutionSummary>> HandleAsync(
        ListExecutionsQuery query,
        CancellationToken cancellationToken)
    {
        var isMember = await tenants
            .IsMemberAsync(query.TenantId, query.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this tenant.");

        var limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 200);

        return await executions
            .ListByWorkflowAsync(query.WorkflowId, query.TenantId, limit, cancellationToken)
            .ConfigureAwait(false);
    }
}

using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Application.Executions.ListExecutions;

public sealed class ListExecutionsHandler(
    IExecutionRepository executions,
    ITenantRepository tenants)
{
    private const int MaxPageSize = 200;
    private const int DefaultPageSize = 20;

    public async Task<PagedResult<ExecutionSummary>> HandleAsync(
        ListExecutionsQuery query,
        CancellationToken cancellationToken)
    {
        var isMember = await tenants
            .IsMemberAsync(query.TenantId, query.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this tenant.");

        if (query.From is { } from && query.To is { } to && from > to)
            throw new ValidationException("'from' must be earlier than or equal to 'to'.");

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0
            ? DefaultPageSize
            : Math.Min(query.PageSize, MaxPageSize);

        var criteria = new ExecutionSearchCriteria(
            query.TenantId,
            query.WorkflowId,
            query.Status,
            query.From,
            query.To,
            page,
            pageSize);

        return await executions.SearchAsync(criteria, cancellationToken).ConfigureAwait(false);
    }
}

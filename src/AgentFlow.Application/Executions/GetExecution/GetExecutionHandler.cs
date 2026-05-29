using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Application.Executions.GetExecution;

public sealed class GetExecutionHandler(
    IExecutionRepository executions,
    ITenantRepository tenants)
{
    public async Task<ExecutionResult> HandleAsync(GetExecutionQuery query, CancellationToken cancellationToken)
    {
        var isMember = await tenants
            .IsMemberAsync(query.TenantId, query.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this tenant.");

        var execution = await executions
            .GetByIdAsync(query.ExecutionId, query.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (execution is null)
            throw new NotFoundException($"Execution '{query.ExecutionId}' not found.");

        return new ExecutionResult(
            execution.Id,
            execution.WorkflowId,
            execution.WorkflowVersion,
            execution.Status.ToString(),
            execution.TriggerPayload,
            execution.ErrorMessage,
            execution.CreatedAtUtc,
            execution.StartedAtUtc,
            execution.CompletedAtUtc,
            execution.Steps
                .OrderBy(s => s.Sequence)
                .Select(s => new StepResult(
                    s.Id,
                    s.NodeId,
                    s.NodeType,
                    s.NodeName,
                    s.Sequence,
                    s.Status.ToString(),
                    s.Input,
                    s.Output,
                    s.ErrorMessage,
                    s.CreatedAtUtc,
                    s.StartedAtUtc,
                    s.CompletedAtUtc))
                .ToList());
    }
}

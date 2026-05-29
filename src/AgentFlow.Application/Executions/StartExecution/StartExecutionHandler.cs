using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Messaging;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Contracts.Events;
using AgentFlow.Domain.Executions;
using AgentFlow.Domain.Workflows;

namespace AgentFlow.Application.Executions.StartExecution;

public sealed class StartExecutionHandler(
    IWorkflowRepository workflows,
    IExecutionRepository executions,
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IMessagePublisher publisher,
    IClock clock)
{
    public async Task<StartExecutionResult> HandleAsync(
        StartExecutionCommand command,
        CancellationToken cancellationToken)
    {
        var isMember = await tenants
            .IsMemberAsync(command.TenantId, command.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this tenant.");

        var workflow = await workflows
            .GetByIdAsync(command.WorkflowId, command.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (workflow is null)
            throw new NotFoundException($"Workflow '{command.WorkflowId}' not found.");

        if (workflow.Status != WorkflowStatus.Published)
            throw new ValidationException($"Workflow must be Published to execute. Current status: {workflow.Status}.");

        if (workflow.Nodes.Count == 0)
            throw new ValidationException("Workflow has no nodes to execute.");

        var execution = WorkflowExecution.Create(
            command.TenantId,
            command.WorkflowId,
            workflow.Version,
            command.TriggerPayload,
            clock.UtcNow);

        executions.Add(execution);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var message = new WorkflowExecutionRequestedV1(
            MessageId: Guid.NewGuid(),
            OccurredAt: clock.UtcNow,
            TenantId: command.TenantId,
            WorkflowId: command.WorkflowId,
            WorkflowVersion: workflow.Version,
            ExecutionId: execution.Id,
            TriggerPayload: command.TriggerPayload);

        await publisher.PublishAsync(message, cancellationToken).ConfigureAwait(false);

        return new StartExecutionResult(
            execution.Id,
            execution.WorkflowId,
            execution.WorkflowVersion,
            execution.Status.ToString(),
            execution.CreatedAtUtc);
    }
}

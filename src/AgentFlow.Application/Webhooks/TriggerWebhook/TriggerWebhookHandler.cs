using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Messaging;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Contracts.Events;
using AgentFlow.Domain.Executions;
using AgentFlow.Domain.Workflows;

namespace AgentFlow.Application.Webhooks.TriggerWebhook;

public sealed class TriggerWebhookHandler(
    IWorkflowRepository workflows,
    IExecutionRepository executions,
    IUnitOfWork unitOfWork,
    IMessagePublisher publisher,
    IClock clock)
{
    public async Task<TriggerWebhookResult> HandleAsync(
        TriggerWebhookCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new NotFoundException("Webhook not found.");

        var workflow = await workflows
            .GetByWebhookTokenAsync(command.Token, cancellationToken)
            .ConfigureAwait(false);

        // Use NotFound (not Forbidden) so we don't leak whether a token exists.
        if (workflow is null)
            throw new NotFoundException("Webhook not found.");

        if (workflow.Status != WorkflowStatus.Published)
            throw new ConflictException($"Workflow must be Published to trigger via webhook. Current status: {workflow.Status}.");

        if (workflow.Nodes.Count == 0)
            throw new ValidationException("Workflow has no nodes to execute.");

        var execution = WorkflowExecution.Create(
            workflow.TenantId,
            workflow.Id,
            workflow.Version,
            command.Payload,
            clock.UtcNow);

        executions.Add(execution);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var message = new WorkflowExecutionRequestedV1(
            MessageId: Guid.NewGuid(),
            OccurredAt: clock.UtcNow,
            TenantId: workflow.TenantId,
            WorkflowId: workflow.Id,
            WorkflowVersion: workflow.Version,
            ExecutionId: execution.Id,
            TriggerPayload: command.Payload);

        await publisher.PublishAsync(message, cancellationToken).ConfigureAwait(false);

        return new TriggerWebhookResult(
            execution.Id,
            execution.WorkflowId,
            execution.TenantId,
            execution.WorkflowVersion,
            execution.Status.ToString(),
            execution.CreatedAtUtc);
    }
}

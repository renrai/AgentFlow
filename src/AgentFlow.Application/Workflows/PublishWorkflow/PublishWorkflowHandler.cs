using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Application.Workflows.PublishWorkflow;

public sealed class PublishWorkflowHandler(
    IWorkflowRepository workflows,
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task HandleAsync(PublishWorkflowCommand command, CancellationToken cancellationToken)
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

        workflow.Publish(clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Domain.Workflows;

namespace AgentFlow.Application.Workflows.CreateWorkflow;

public sealed class CreateWorkflowHandler(
    IWorkflowRepository workflows,
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<CreateWorkflowResult> HandleAsync(
        CreateWorkflowCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Workflow name is required.");

        var isMember = await tenants
            .IsMemberAsync(command.TenantId, command.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this tenant.");

        var workflow = Workflow.Create(command.TenantId, command.Name, command.Description, clock.UtcNow);

        workflow.UpdateDefinition(
            command.Name,
            command.Description,
            command.Nodes,
            command.Edges,
            clock.UtcNow);

        workflows.Add(workflow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateWorkflowResult(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Status.ToString(),
            workflow.Version,
            workflow.WebhookToken,
            workflow.CreatedAtUtc);
    }
}

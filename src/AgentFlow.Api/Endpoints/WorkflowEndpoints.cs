using AgentFlow.Application.Abstractions.Identity;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Application.Workflows.ArchiveWorkflow;
using AgentFlow.Application.Workflows.CreateWorkflow;
using AgentFlow.Application.Workflows.GetWorkflow;
using AgentFlow.Application.Workflows.ListWorkflows;
using AgentFlow.Application.Workflows.PublishWorkflow;
using AgentFlow.Application.Workflows.UpdateWorkflow;
using AgentFlow.Domain.Workflows;

namespace AgentFlow.Api.Endpoints;

public static class WorkflowEndpoints
{
    public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants/{tenantId:guid}/workflows")
            .WithTags("Workflows")
            .RequireAuthorization();

        group.MapPost("/", async (
            Guid tenantId,
            CreateWorkflowRequest request,
            ICurrentUser currentUser,
            CreateWorkflowHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var command = new CreateWorkflowCommand(
                tenantId,
                userId,
                request.Name,
                request.Description,
                request.Nodes?.Select(n => new NodeDefinition(n.Type, n.Name, n.PositionX, n.PositionY, n.Configuration ?? "{}")).ToList() ?? [],
                request.Edges?.Select(e => new EdgeDefinition(e.SourceNodeId, e.TargetNodeId, e.Label)).ToList() ?? []);

            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.Created($"/tenants/{tenantId}/workflows/{result.WorkflowId}", result);
        })
        .WithName("CreateWorkflow")
        .WithSummary("Creates a new workflow in draft state.");

        group.MapGet("/", async (
            Guid tenantId,
            ICurrentUser currentUser,
            ListWorkflowsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var result = await handler.HandleAsync(new ListWorkflowsQuery(tenantId, userId), cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("ListWorkflows")
        .WithSummary("Lists all workflows in a tenant.");

        group.MapGet("/{workflowId:guid}", async (
            Guid tenantId,
            Guid workflowId,
            ICurrentUser currentUser,
            GetWorkflowHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var result = await handler.HandleAsync(new GetWorkflowQuery(workflowId, tenantId, userId), cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("GetWorkflow")
        .WithSummary("Returns a workflow with its full node and edge definition.");

        group.MapPut("/{workflowId:guid}", async (
            Guid tenantId,
            Guid workflowId,
            UpdateWorkflowRequest request,
            ICurrentUser currentUser,
            UpdateWorkflowHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var command = new UpdateWorkflowCommand(
                workflowId,
                tenantId,
                userId,
                request.Name,
                request.Description,
                request.Nodes?.Select(n => new NodeDefinition(n.Type, n.Name, n.PositionX, n.PositionY, n.Configuration ?? "{}")).ToList() ?? [],
                request.Edges?.Select(e => new EdgeDefinition(e.SourceNodeId, e.TargetNodeId, e.Label)).ToList() ?? []);

            await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.NoContent();
        })
        .WithName("UpdateWorkflow")
        .WithSummary("Replaces the workflow definition (name, nodes, edges).");

        group.MapPost("/{workflowId:guid}/publish", async (
            Guid tenantId,
            Guid workflowId,
            ICurrentUser currentUser,
            PublishWorkflowHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            await handler.HandleAsync(new PublishWorkflowCommand(workflowId, tenantId, userId), cancellationToken).ConfigureAwait(false);
            return Results.NoContent();
        })
        .WithName("PublishWorkflow")
        .WithSummary("Publishes a draft workflow, making it executable.");

        group.MapPost("/{workflowId:guid}/archive", async (
            Guid tenantId,
            Guid workflowId,
            ICurrentUser currentUser,
            ArchiveWorkflowHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            await handler.HandleAsync(new ArchiveWorkflowCommand(workflowId, tenantId, userId), cancellationToken).ConfigureAwait(false);
            return Results.NoContent();
        })
        .WithName("ArchiveWorkflow")
        .WithSummary("Archives a workflow, preventing further modifications.");

        return app;
    }

    private static Guid RequireUserId(ICurrentUser currentUser)
    {
        if (currentUser.UserId is not { } userId)
            throw new AuthenticationException("Authenticated user is missing a valid identifier.");
        return userId;
    }
}

public sealed record CreateWorkflowRequest(
    string Name,
    string? Description,
    IReadOnlyList<NodeRequest>? Nodes,
    IReadOnlyList<EdgeRequest>? Edges);

public sealed record UpdateWorkflowRequest(
    string Name,
    string? Description,
    IReadOnlyList<NodeRequest>? Nodes,
    IReadOnlyList<EdgeRequest>? Edges);

public sealed record NodeRequest(
    string Type,
    string Name,
    double PositionX,
    double PositionY,
    string? Configuration);

public sealed record EdgeRequest(
    Guid SourceNodeId,
    Guid TargetNodeId,
    string? Label);

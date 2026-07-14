using AgentFlow.Application.Abstractions.Identity;
using AgentFlow.Application.Executions.GetExecution;
using AgentFlow.Application.Executions.ListExecutions;
using AgentFlow.Application.Executions.StartExecution;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Domain.Executions;

namespace AgentFlow.Api.Endpoints;

public static class ExecutionEndpoints
{
    public static IEndpointRouteBuilder MapExecutionEndpoints(this IEndpointRouteBuilder app)
    {
        var workflowGroup = app.MapGroup("/tenants/{tenantId:guid}/workflows/{workflowId:guid}/executions")
            .WithTags("Executions")
            .RequireAuthorization();

        workflowGroup.MapPost("/", async (
            Guid tenantId,
            Guid workflowId,
            StartExecutionRequest request,
            ICurrentUser currentUser,
            StartExecutionHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var command = new StartExecutionCommand(workflowId, tenantId, userId, request.TriggerPayload);
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.Accepted($"/tenants/{tenantId}/executions/{result.ExecutionId}", result);
        })
        .WithName("StartExecution")
        .WithSummary("Starts a new execution of a published workflow.");

        workflowGroup.MapGet("/", async (
            Guid tenantId,
            Guid workflowId,
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int? page,
            int? pageSize,
            ICurrentUser currentUser,
            ListExecutionsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var query = new ListExecutionsQuery(
                tenantId,
                userId,
                workflowId,
                ParseStatus(status),
                from,
                to,
                page ?? 1,
                pageSize ?? 20);

            var result = await handler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("ListWorkflowExecutions")
        .WithSummary("Lists executions of a specific workflow with optional filters.");

        var tenantGroup = app.MapGroup("/tenants/{tenantId:guid}/executions")
            .WithTags("Executions")
            .RequireAuthorization();

        tenantGroup.MapGet("/", async (
            Guid tenantId,
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int? page,
            int? pageSize,
            ICurrentUser currentUser,
            ListExecutionsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var query = new ListExecutionsQuery(
                tenantId,
                userId,
                WorkflowId: null,
                ParseStatus(status),
                from,
                to,
                page ?? 1,
                pageSize ?? 20);

            var result = await handler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("ListTenantExecutions")
        .WithSummary("Lists all executions in a tenant with optional filters.");

        tenantGroup.MapGet("/{executionId:guid}", async (
            Guid tenantId,
            Guid executionId,
            ICurrentUser currentUser,
            GetExecutionHandler handler,
            CancellationToken cancellationToken) =>
        {
            var userId = RequireUserId(currentUser);
            var result = await handler.HandleAsync(new GetExecutionQuery(executionId, tenantId, userId), cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("GetExecution")
        .WithSummary("Returns an execution with all its steps.");

        return app;
    }

    private static ExecutionStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        if (Enum.TryParse<ExecutionStatus>(status, ignoreCase: true, out var parsed))
            return parsed;

        throw new ValidationException(
            $"Invalid status '{status}'. Valid values: {string.Join(", ", Enum.GetNames<ExecutionStatus>())}.");
    }

    private static Guid RequireUserId(ICurrentUser currentUser)
    {
        if (currentUser.UserId is not { } userId)
            throw new AuthenticationException("Authenticated user is missing a valid identifier.");
        return userId;
    }
}

public sealed record StartExecutionRequest(string? TriggerPayload);

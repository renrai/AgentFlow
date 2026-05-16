using AgentFlow.Application.Abstractions.Identity;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Application.Tenants.CreateTenant;
using AgentFlow.Application.Tenants.GetMyTenants;

namespace AgentFlow.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants")
            .WithTags("Tenants")
            .RequireAuthorization();

        group.MapPost("/", async (
            CreateTenantRequest request,
            ICurrentUser currentUser,
            CreateTenantHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (currentUser.UserId is not { } userId)
            {
                throw new AuthenticationException("authenticated user is missing a valid identifier.");
            }

            var command = new CreateTenantCommand(userId, request.Name, request.Slug);
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.Created($"/tenants/{result.TenantId}", result);
        })
        .WithName("CreateTenant")
        .WithSummary("Creates a new tenant owned by the authenticated user.");

        group.MapGet("/me", async (
            ICurrentUser currentUser,
            GetMyTenantsHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (currentUser.UserId is not { } userId)
            {
                throw new AuthenticationException("authenticated user is missing a valid identifier.");
            }

            var result = await handler.HandleAsync(new GetMyTenantsQuery(userId), cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("GetMyTenants")
        .WithSummary("Returns tenants the authenticated user is a member of.");

        return app;
    }
}

public sealed record CreateTenantRequest(string Name, string? Slug);

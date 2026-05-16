using AgentFlow.Application.Abstractions.Identity;
using AgentFlow.Application.Identity.GetCurrentUser;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/me")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", async (
            ICurrentUser currentUser,
            GetCurrentUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (currentUser.UserId is not { } userId)
            {
                throw new AuthenticationException("authenticated user is missing a valid identifier.");
            }

            var result = await handler.HandleAsync(new GetCurrentUserQuery(userId), cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("GetCurrentUser")
        .WithSummary("Returns the authenticated user and their tenant memberships.");

        return app;
    }
}

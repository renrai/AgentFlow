using AgentFlow.Application.Identity.Login;
using AgentFlow.Application.Identity.Register;

namespace AgentFlow.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth")
            .AllowAnonymous();

        group.MapPost("/register", async (
            RegisterRequest request,
            RegisterUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.DisplayName,
                request.TenantName,
                request.TenantSlug);

            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.Created($"/users/{result.UserId}", result);
        })
        .WithName("Register")
        .WithSummary("Registers a new user and creates their initial tenant.");

        group.MapPost("/login", async (
            LoginRequest request,
            LoginHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Authenticates a user and returns a JWT access token.");

        return app;
    }
}

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string TenantName,
    string? TenantSlug);

public sealed record LoginRequest(string Email, string Password);

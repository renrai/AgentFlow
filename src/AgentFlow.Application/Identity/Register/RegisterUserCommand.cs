namespace AgentFlow.Application.Identity.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string TenantName,
    string? TenantSlug = null);

public sealed record RegisterUserResult(
    Guid UserId,
    Guid TenantId,
    string Email,
    string TenantSlug);

using AgentFlow.Domain.Tenants;

namespace AgentFlow.Application.Identity.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId);

public sealed record CurrentUserResult(
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<CurrentUserMembership> Memberships);

public sealed record CurrentUserMembership(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    TenantMemberRole Role,
    TenantMemberStatus Status);

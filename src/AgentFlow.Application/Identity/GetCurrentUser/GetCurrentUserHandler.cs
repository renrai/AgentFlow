using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;

namespace AgentFlow.Application.Identity.GetCurrentUser;

public sealed class GetCurrentUserHandler(
    IUserRepository users,
    ITenantRepository tenants)
{
    public async Task<CurrentUserResult> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await users.GetByIdAsync(query.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("user not found.");

        var memberships = await tenants
            .GetMembershipsByUserAsync(query.UserId, cancellationToken)
            .ConfigureAwait(false);

        var mapped = memberships
            .Select(m => new CurrentUserMembership(m.TenantId, m.TenantName, m.TenantSlug, m.Role, m.Status))
            .ToList();

        return new CurrentUserResult(user.Id, user.Email, user.DisplayName, mapped);
    }
}

using AgentFlow.Application.Abstractions.Persistence;

namespace AgentFlow.Application.Tenants.GetMyTenants;

public sealed class GetMyTenantsHandler(ITenantRepository tenants)
{
    public async Task<IReadOnlyList<MyTenantSummary>> HandleAsync(
        GetMyTenantsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var memberships = await tenants
            .GetMembershipsByUserAsync(query.UserId, cancellationToken)
            .ConfigureAwait(false);

        return memberships
            .Select(m => new MyTenantSummary(m.TenantId, m.TenantName, m.TenantSlug, m.Role, m.Status))
            .ToList();
    }
}

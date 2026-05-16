using AgentFlow.Domain.Tenants;

namespace AgentFlow.Application.Tenants.GetMyTenants;

public sealed record GetMyTenantsQuery(Guid UserId);

public sealed record MyTenantSummary(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    TenantMemberRole Role,
    TenantMemberStatus Status);

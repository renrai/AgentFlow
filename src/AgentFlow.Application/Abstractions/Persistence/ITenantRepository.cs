using AgentFlow.Domain.Tenants;

namespace AgentFlow.Application.Abstractions.Persistence;

public interface ITenantRepository
{
    void Add(Tenant tenant);

    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantMembershipSummary>> GetMembershipsByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> IsMemberAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

public sealed record TenantMembershipSummary(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    TenantMemberRole Role,
    TenantMemberStatus Status);

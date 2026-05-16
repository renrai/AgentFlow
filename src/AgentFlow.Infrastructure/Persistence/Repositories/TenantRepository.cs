using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository(AgentFlowDbContext dbContext) : ITenantRepository
{
    public void Add(Tenant tenant) => dbContext.Tenants.Add(tenant);

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Tenants
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSlug(slug);
        return dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == normalized, cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSlug(slug);
        return dbContext.Tenants.AnyAsync(t => t.Slug == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<TenantMembershipSummary>> GetMembershipsByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query =
            from member in dbContext.TenantMembers.AsNoTracking()
            join tenant in dbContext.Tenants.AsNoTracking() on member.TenantId equals tenant.Id
            where member.UserId == userId && member.Status != TenantMemberStatus.Removed
            select new TenantMembershipSummary(
                tenant.Id,
                tenant.Name,
                tenant.Slug,
                member.Role,
                member.Status);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> IsMemberAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        => dbContext.TenantMembers
            .AnyAsync(
                m => m.TenantId == tenantId && m.UserId == userId && m.Status != TenantMemberStatus.Removed,
                cancellationToken);

    private static string NormalizeSlug(string slug)
        => string.IsNullOrWhiteSpace(slug) ? string.Empty : slug.Trim().ToLowerInvariant();
}

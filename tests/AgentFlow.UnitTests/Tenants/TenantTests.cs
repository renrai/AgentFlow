using AgentFlow.Domain.SharedKernel;
using AgentFlow.Domain.Tenants;

namespace AgentFlow.UnitTests.Tenants;

public sealed class TenantTests
{
    [Fact]
    public void CreateShouldNormalizeSlugAndCreateOwnerMembership()
    {
        var ownerUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var tenant = Tenant.Create("Acme Operations", " Acme-Ops ", ownerUserId, now);
        var ownerMembership = Assert.Single(tenant.Members);

        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.Equal("Acme Operations", tenant.Name);
        Assert.Equal("acme-ops", tenant.Slug);
        Assert.Equal(TenantStatus.Active, tenant.Status);
        Assert.Equal(TenantMemberRole.Owner, ownerMembership.Role);
        Assert.Equal(TenantMemberStatus.Active, ownerMembership.Status);
        Assert.Equal(ownerUserId, ownerMembership.UserId);
    }

    [Fact]
    public void InviteMemberShouldRejectDuplicateActiveMembership()
    {
        var userId = Guid.NewGuid();
        var tenant = Tenant.Create("Acme Operations", "acme-ops", Guid.NewGuid(), DateTimeOffset.UtcNow);

        tenant.InviteMember(userId, TenantMemberRole.Member, DateTimeOffset.UtcNow);

        var exception = Assert.Throws<DomainException>(() =>
            tenant.InviteMember(userId, TenantMemberRole.Admin, DateTimeOffset.UtcNow));

        Assert.Contains("already has a membership", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OwnerMembershipCannotBeRemoved()
    {
        var tenant = Tenant.Create("Acme Operations", "acme-ops", Guid.NewGuid(), DateTimeOffset.UtcNow);
        var ownerMembership = Assert.Single(tenant.Members);

        var exception = Assert.Throws<DomainException>(() =>
            ownerMembership.Remove(DateTimeOffset.UtcNow));

        Assert.Contains("owner cannot be removed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}

using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Tenants;

public sealed class TenantMember : Entity<Guid>
{
    private TenantMember()
        : base(Guid.Empty)
    {
    }

    private TenantMember(
        Guid id,
        Guid tenantId,
        Guid userId,
        TenantMemberRole role,
        TenantMemberStatus status,
        DateTimeOffset invitedAtUtc,
        DateTimeOffset? joinedAtUtc)
        : base(id)
    {
        TenantId = Guard.NotEmpty(tenantId, nameof(tenantId));
        UserId = Guard.NotEmpty(userId, nameof(userId));
        Role = role;
        Status = status;
        InvitedAtUtc = invitedAtUtc;
        JoinedAtUtc = joinedAtUtc;
    }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public TenantMemberRole Role { get; private set; }

    public TenantMemberStatus Status { get; private set; }

    public DateTimeOffset InvitedAtUtc { get; private set; }

    public DateTimeOffset? JoinedAtUtc { get; private set; }

    public DateTimeOffset? RemovedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public bool IsActive => Status == TenantMemberStatus.Active;

    public static TenantMember CreateOwner(Guid tenantId, Guid userId, DateTimeOffset utcNow)
    {
        return new TenantMember(
            Guid.NewGuid(),
            tenantId,
            userId,
            TenantMemberRole.Owner,
            TenantMemberStatus.Active,
            utcNow,
            utcNow);
    }

    public static TenantMember Invite(
        Guid tenantId,
        Guid userId,
        TenantMemberRole role,
        DateTimeOffset utcNow)
    {
        if (role == TenantMemberRole.Owner)
        {
            throw new DomainException("owner memberships can only be created with the tenant.");
        }

        return new TenantMember(
            Guid.NewGuid(),
            tenantId,
            userId,
            role,
            TenantMemberStatus.Pending,
            utcNow,
            joinedAtUtc: null);
    }

    public void AcceptInvitation(DateTimeOffset utcNow)
    {
        if (Status == TenantMemberStatus.Removed)
        {
            throw new DomainException("removed memberships cannot be accepted.");
        }

        Status = TenantMemberStatus.Active;
        JoinedAtUtc ??= utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void ChangeRole(TenantMemberRole role, DateTimeOffset utcNow)
    {
        if (Role == TenantMemberRole.Owner)
        {
            throw new DomainException("tenant owner role cannot be changed.");
        }

        if (role == TenantMemberRole.Owner)
        {
            throw new DomainException("owner role cannot be assigned through membership update.");
        }

        Role = role;
        UpdatedAtUtc = utcNow;
    }

    public void Remove(DateTimeOffset utcNow)
    {
        if (Role == TenantMemberRole.Owner)
        {
            throw new DomainException("tenant owner cannot be removed from the tenant.");
        }

        if (Status == TenantMemberStatus.Removed)
        {
            return;
        }

        Status = TenantMemberStatus.Removed;
        RemovedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }
}

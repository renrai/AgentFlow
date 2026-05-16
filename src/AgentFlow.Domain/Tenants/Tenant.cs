using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Tenants;

public sealed class Tenant : AggregateRoot<Guid>
{
    private const int NameMaxLength = 160;
    private const int SlugMaxLength = 80;

    private readonly List<TenantMember> _members = [];

    private Tenant()
        : base(Guid.Empty)
    {
    }

    private Tenant(
        Guid id,
        string name,
        string slug,
        Guid ownerUserId,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = Guard.NotBlank(name, nameof(name), NameMaxLength);
        Slug = NormalizeSlug(slug);
        Status = TenantStatus.Active;
        CreatedAtUtc = createdAtUtc;

        _members.Add(TenantMember.CreateOwner(Id, ownerUserId, createdAtUtc));
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public TenantStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<TenantMember> Members => _members.AsReadOnly();

    public bool IsActive => Status == TenantStatus.Active;

    public static Tenant Create(
        string name,
        string slug,
        Guid ownerUserId,
        DateTimeOffset utcNow)
    {
        Guard.NotEmpty(ownerUserId, nameof(ownerUserId));

        return new Tenant(Guid.NewGuid(), name, slug, ownerUserId, utcNow);
    }

    public TenantMember InviteMember(
        Guid userId,
        TenantMemberRole role,
        DateTimeOffset utcNow)
    {
        EnsureActive();
        Guard.NotEmpty(userId, nameof(userId));

        if (_members.Any(member => member.UserId == userId && member.Status != TenantMemberStatus.Removed))
        {
            throw new DomainException("user already has a membership in this tenant.");
        }

        var member = TenantMember.Invite(Id, userId, role, utcNow);
        _members.Add(member);
        UpdatedAtUtc = utcNow;

        return member;
    }

    public TenantMember? FindMembership(Guid userId)
    {
        return _members.FirstOrDefault(member => member.UserId == userId && member.Status != TenantMemberStatus.Removed);
    }

    public void Rename(string name, DateTimeOffset utcNow)
    {
        EnsureActive();

        Name = Guard.NotBlank(name, nameof(name), NameMaxLength);
        UpdatedAtUtc = utcNow;
    }

    public void Suspend(DateTimeOffset utcNow)
    {
        if (Status == TenantStatus.Suspended)
        {
            return;
        }

        Status = TenantStatus.Suspended;
        UpdatedAtUtc = utcNow;
    }

    public void Activate(DateTimeOffset utcNow)
    {
        if (Status == TenantStatus.Active)
        {
            return;
        }

        Status = TenantStatus.Active;
        UpdatedAtUtc = utcNow;
    }

    private void EnsureActive()
    {
        if (Status != TenantStatus.Active)
        {
            throw new DomainException("tenant must be active.");
        }
    }

    private static string NormalizeSlug(string slug)
    {
        var normalized = Guard.NotBlank(slug, nameof(slug), SlugMaxLength).ToLowerInvariant();

        if (normalized.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            throw new DomainException("slug can contain only letters, numbers, and hyphens.");
        }

        if (normalized.StartsWith('-') || normalized.EndsWith('-'))
        {
            throw new DomainException("slug cannot start or end with a hyphen.");
        }

        return normalized;
    }
}

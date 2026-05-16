using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Users;

public sealed class User : AggregateRoot<Guid>
{
    private const int EmailMaxLength = 320;
    private const int DisplayNameMaxLength = 160;
    private const int PasswordHashMaxLength = 512;

    private User()
        : base(Guid.Empty)
    {
    }

    private User(
        Guid id,
        string email,
        string displayName,
        string passwordHash,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Email = NormalizeEmail(email);
        DisplayName = Guard.NotBlank(displayName, nameof(displayName), DisplayNameMaxLength);
        PasswordHash = Guard.NotBlank(passwordHash, nameof(passwordHash), PasswordHashMaxLength);
        Status = UserStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    public bool IsActive => Status == UserStatus.Active;

    public static User Create(
        string email,
        string displayName,
        string passwordHash,
        DateTimeOffset utcNow)
    {
        return new User(Guid.NewGuid(), email, displayName, passwordHash, utcNow);
    }

    public void Rename(string displayName, DateTimeOffset utcNow)
    {
        DisplayName = Guard.NotBlank(displayName, nameof(displayName), DisplayNameMaxLength);
        UpdatedAtUtc = utcNow;
    }

    public void ChangePasswordHash(string passwordHash, DateTimeOffset utcNow)
    {
        PasswordHash = Guard.NotBlank(passwordHash, nameof(passwordHash), PasswordHashMaxLength);
        UpdatedAtUtc = utcNow;
    }

    public void MarkLogin(DateTimeOffset utcNow)
    {
        LastLoginAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void Deactivate(DateTimeOffset utcNow)
    {
        if (Status == UserStatus.Deactivated)
        {
            return;
        }

        Status = UserStatus.Deactivated;
        UpdatedAtUtc = utcNow;
    }

    public void Reactivate(DateTimeOffset utcNow)
    {
        if (Status == UserStatus.Active)
        {
            return;
        }

        Status = UserStatus.Active;
        UpdatedAtUtc = utcNow;
    }

    private static string NormalizeEmail(string email)
    {
        var normalized = Guard.NotBlank(email, nameof(email), EmailMaxLength).ToLowerInvariant();

        if (!normalized.Contains('@', StringComparison.Ordinal) || normalized.StartsWith('@') || normalized.EndsWith('@'))
        {
            throw new DomainException("email must be a valid email address.");
        }

        return normalized;
    }
}

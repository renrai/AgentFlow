using System.Text;
using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Identity;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Domain.Tenants;
using AgentFlow.Domain.Users;

namespace AgentFlow.Application.Identity.Register;

public sealed class RegisterUserHandler(
    IUserRepository users,
    ITenantRepository tenants,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    private const int MinPasswordLength = 8;

    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("email is required.");
        }

        if (string.IsNullOrEmpty(command.Password) || command.Password.Length < MinPasswordLength)
        {
            throw new ValidationException($"password must be at least {MinPasswordLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            throw new ValidationException("display name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.TenantName))
        {
            throw new ValidationException("tenant name is required.");
        }

        var slug = !string.IsNullOrWhiteSpace(command.TenantSlug)
            ? command.TenantSlug.Trim().ToLowerInvariant()
            : Slugify(command.TenantName);

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ValidationException("tenant slug could not be derived from the tenant name.");
        }

        if (await users.ExistsByEmailAsync(command.Email, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("a user with this email already exists.");
        }

        if (await tenants.ExistsBySlugAsync(slug, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("a tenant with this slug already exists.");
        }

        var now = clock.UtcNow;
        var hash = passwordHasher.Hash(command.Password);

        var user = User.Create(command.Email, command.DisplayName, hash, now);
        var tenant = Tenant.Create(command.TenantName, slug, user.Id, now);

        users.Add(user);
        tenants.Add(tenant);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RegisterUserResult(user.Id, tenant.Id, user.Email, tenant.Slug);
    }

    private static string Slugify(string input)
    {
        var lower = input.Trim().ToLowerInvariant();
        var builder = new StringBuilder(lower.Length);
        var lastDash = false;

        foreach (var character in lower)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
                lastDash = false;
            }
            else if (!lastDash && builder.Length > 0)
            {
                builder.Append('-');
                lastDash = true;
            }
        }

        while (builder.Length > 0 && builder[^1] == '-')
        {
            builder.Length--;
        }

        return builder.ToString();
    }
}

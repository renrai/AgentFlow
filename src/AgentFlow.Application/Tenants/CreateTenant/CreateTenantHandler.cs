using System.Text;
using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Domain.Tenants;

namespace AgentFlow.Application.Tenants.CreateTenant;

public sealed class CreateTenantHandler(
    IUserRepository users,
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<CreateTenantResult> HandleAsync(
        CreateTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OwnerUserId == Guid.Empty)
        {
            throw new ValidationException("owner user id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("tenant name is required.");
        }

        var owner = await users.GetByIdAsync(command.OwnerUserId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("owner user not found.");

        var slug = !string.IsNullOrWhiteSpace(command.Slug)
            ? command.Slug.Trim().ToLowerInvariant()
            : Slugify(command.Name);

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ValidationException("tenant slug could not be derived from the tenant name.");
        }

        if (await tenants.ExistsBySlugAsync(slug, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("a tenant with this slug already exists.");
        }

        var tenant = Tenant.Create(command.Name, slug, owner.Id, clock.UtcNow);
        tenants.Add(tenant);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateTenantResult(tenant.Id, tenant.Name, tenant.Slug);
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

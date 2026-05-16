namespace AgentFlow.Application.Tenants.CreateTenant;

public sealed record CreateTenantCommand(
    Guid OwnerUserId,
    string Name,
    string? Slug);

public sealed record CreateTenantResult(Guid TenantId, string Name, string Slug);

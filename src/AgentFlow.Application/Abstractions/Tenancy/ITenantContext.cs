namespace AgentFlow.Application.Abstractions.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }

    bool HasTenant => TenantId.HasValue;
}

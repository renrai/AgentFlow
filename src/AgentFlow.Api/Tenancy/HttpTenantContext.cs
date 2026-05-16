using AgentFlow.Application.Abstractions.Tenancy;

namespace AgentFlow.Api.Tenancy;

public sealed class HttpTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    public const string TenantHeaderName = "X-Tenant-Id";

    private readonly IHttpContextAccessor _accessor = accessor;

    public Guid? TenantId
    {
        get
        {
            var context = _accessor.HttpContext;
            if (context is null)
            {
                return null;
            }

            if (!context.Request.Headers.TryGetValue(TenantHeaderName, out var raw) || raw.Count == 0)
            {
                return null;
            }

            return Guid.TryParse(raw.ToString(), out var id) ? id : null;
        }
    }
}

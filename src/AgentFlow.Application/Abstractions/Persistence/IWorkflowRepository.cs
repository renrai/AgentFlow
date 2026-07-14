using AgentFlow.Domain.Workflows;

namespace AgentFlow.Application.Abstractions.Persistence;

public interface IWorkflowRepository
{
    void Add(Workflow workflow);

    Task<Workflow?> GetByIdAsync(Guid workflowId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<Workflow?> GetByWebhookTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowSummary>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed record WorkflowSummary(
    Guid WorkflowId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset UpdatedAtUtc);

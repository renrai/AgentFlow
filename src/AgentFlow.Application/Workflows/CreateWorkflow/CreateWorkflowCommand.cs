using AgentFlow.Domain.Workflows;

namespace AgentFlow.Application.Workflows.CreateWorkflow;

public sealed record CreateWorkflowCommand(
    Guid TenantId,
    Guid UserId,
    string Name,
    string? Description,
    IReadOnlyList<NodeDefinition> Nodes,
    IReadOnlyList<EdgeDefinition> Edges);

public sealed record CreateWorkflowResult(
    Guid WorkflowId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset CreatedAtUtc);

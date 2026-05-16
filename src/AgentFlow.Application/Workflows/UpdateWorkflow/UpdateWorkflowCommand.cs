using AgentFlow.Domain.Workflows;

namespace AgentFlow.Application.Workflows.UpdateWorkflow;

public sealed record UpdateWorkflowCommand(
    Guid WorkflowId,
    Guid TenantId,
    Guid UserId,
    string Name,
    string? Description,
    IReadOnlyList<NodeDefinition> Nodes,
    IReadOnlyList<EdgeDefinition> Edges);

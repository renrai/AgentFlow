namespace AgentFlow.Application.Workflows.GetWorkflow;

public sealed record GetWorkflowQuery(Guid WorkflowId, Guid TenantId, Guid UserId);

public sealed record WorkflowResult(
    Guid WorkflowId,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<NodeResult> Nodes,
    IReadOnlyList<EdgeResult> Edges);

public sealed record NodeResult(
    Guid NodeId,
    string Type,
    string Name,
    double PositionX,
    double PositionY,
    string Configuration);

public sealed record EdgeResult(
    Guid EdgeId,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string? Label);

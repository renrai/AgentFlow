using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Workflows;

public sealed class WorkflowEdge : Entity<Guid>
{
    private const int LabelMaxLength = 100;

    private WorkflowEdge()
        : base(Guid.Empty)
    {
    }

    private WorkflowEdge(
        Guid id,
        Guid workflowId,
        Guid sourceNodeId,
        Guid targetNodeId,
        string? label)
        : base(id)
    {
        WorkflowId = Guard.NotEmpty(workflowId, nameof(workflowId));
        SourceNodeId = Guard.NotEmpty(sourceNodeId, nameof(sourceNodeId));
        TargetNodeId = Guard.NotEmpty(targetNodeId, nameof(targetNodeId));

        if (label is not null)
        {
            Guard.NotBlank(label, nameof(label), LabelMaxLength);
        }

        Label = label;
    }

    public Guid WorkflowId { get; private set; }
    public Guid SourceNodeId { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public string? Label { get; private set; }

    internal static WorkflowEdge Create(
        Guid workflowId,
        Guid sourceNodeId,
        Guid targetNodeId,
        string? label)
    {
        return new WorkflowEdge(Guid.NewGuid(), workflowId, sourceNodeId, targetNodeId, label);
    }
}

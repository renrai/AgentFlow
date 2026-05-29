using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Executions;

public sealed class ExecutionStep : Entity<Guid>
{
    private const int NodeTypeMaxLength = 100;
    private const int NodeNameMaxLength = 200;

    private ExecutionStep()
        : base(Guid.Empty)
    {
    }

    private ExecutionStep(
        Guid id,
        Guid workflowExecutionId,
        Guid nodeId,
        string nodeType,
        string nodeName,
        int sequence,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        WorkflowExecutionId = Guard.NotEmpty(workflowExecutionId, nameof(workflowExecutionId));
        NodeId = Guard.NotEmpty(nodeId, nameof(nodeId));
        NodeType = Guard.NotBlank(nodeType, nameof(nodeType), NodeTypeMaxLength);
        NodeName = Guard.NotBlank(nodeName, nameof(nodeName), NodeNameMaxLength);
        Sequence = sequence;
        Status = StepStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid WorkflowExecutionId { get; private set; }
    public Guid NodeId { get; private set; }
    public string NodeType { get; private set; } = string.Empty;
    public string NodeName { get; private set; } = string.Empty;
    public int Sequence { get; private set; }
    public StepStatus Status { get; private set; }
    public string? Input { get; private set; }
    public string? Output { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    internal static ExecutionStep Create(
        Guid workflowExecutionId,
        Guid nodeId,
        string nodeType,
        string nodeName,
        int sequence,
        DateTimeOffset utcNow)
    {
        return new ExecutionStep(Guid.NewGuid(), workflowExecutionId, nodeId, nodeType, nodeName, sequence, utcNow);
    }

    public void MarkRunning(string? input, DateTimeOffset utcNow)
    {
        if (Status != StepStatus.Pending)
            throw new DomainException($"Step cannot transition from {Status} to Running.");

        Input = input;
        Status = StepStatus.Running;
        StartedAtUtc = utcNow;
    }

    public void MarkCompleted(string? output, DateTimeOffset utcNow)
    {
        if (Status != StepStatus.Running)
            throw new DomainException($"Step cannot transition from {Status} to Completed.");

        Output = output;
        Status = StepStatus.Completed;
        CompletedAtUtc = utcNow;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset utcNow)
    {
        if (Status is StepStatus.Completed or StepStatus.Skipped)
            throw new DomainException($"Step cannot transition from {Status} to Failed.");

        ErrorMessage = errorMessage;
        Status = StepStatus.Failed;
        CompletedAtUtc = utcNow;
    }

    public void MarkSkipped(DateTimeOffset utcNow)
    {
        if (Status != StepStatus.Pending)
            throw new DomainException($"Step cannot transition from {Status} to Skipped.");

        Status = StepStatus.Skipped;
        CompletedAtUtc = utcNow;
    }
}

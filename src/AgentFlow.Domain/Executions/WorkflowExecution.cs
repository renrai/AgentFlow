using AgentFlow.Domain.SharedKernel;

namespace AgentFlow.Domain.Executions;

public sealed class WorkflowExecution : AggregateRoot<Guid>
{
    private const int ErrorMessageMaxLength = 4000;

    private readonly List<ExecutionStep> _steps = [];

    private WorkflowExecution()
        : base(Guid.Empty)
    {
    }

    private WorkflowExecution(
        Guid id,
        Guid tenantId,
        Guid workflowId,
        int workflowVersion,
        string? triggerPayload,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = Guard.NotEmpty(tenantId, nameof(tenantId));
        WorkflowId = Guard.NotEmpty(workflowId, nameof(workflowId));
        WorkflowVersion = workflowVersion;
        TriggerPayload = triggerPayload;
        Status = ExecutionStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid TenantId { get; private set; }
    public Guid WorkflowId { get; private set; }
    public int WorkflowVersion { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public string? TriggerPayload { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public IReadOnlyCollection<ExecutionStep> Steps => _steps.AsReadOnly();

    public static WorkflowExecution Create(
        Guid tenantId,
        Guid workflowId,
        int workflowVersion,
        string? triggerPayload,
        DateTimeOffset utcNow)
    {
        return new WorkflowExecution(Guid.NewGuid(), tenantId, workflowId, workflowVersion, triggerPayload, utcNow);
    }

    public ExecutionStep AddStep(Guid nodeId, string nodeType, string nodeName, DateTimeOffset utcNow)
    {
        var step = ExecutionStep.Create(Id, nodeId, nodeType, nodeName, _steps.Count, utcNow);
        _steps.Add(step);
        return step;
    }

    public void MarkRunning(DateTimeOffset utcNow)
    {
        if (Status != ExecutionStatus.Pending)
            throw new DomainException($"Execution cannot transition from {Status} to Running.");

        Status = ExecutionStatus.Running;
        StartedAtUtc = utcNow;
    }

    public void MarkCompleted(DateTimeOffset utcNow)
    {
        if (Status != ExecutionStatus.Running)
            throw new DomainException($"Execution cannot transition from {Status} to Completed.");

        Status = ExecutionStatus.Completed;
        CompletedAtUtc = utcNow;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset utcNow)
    {
        if (Status is ExecutionStatus.Completed or ExecutionStatus.Cancelled)
            throw new DomainException($"Execution cannot transition from {Status} to Failed.");

        ErrorMessage = Guard.NotBlank(errorMessage, nameof(errorMessage), ErrorMessageMaxLength);
        Status = ExecutionStatus.Failed;
        CompletedAtUtc = utcNow;
    }

    public void Cancel(DateTimeOffset utcNow)
    {
        if (Status is ExecutionStatus.Completed or ExecutionStatus.Failed or ExecutionStatus.Cancelled)
            throw new DomainException($"Execution cannot transition from {Status} to Cancelled.");

        Status = ExecutionStatus.Cancelled;
        CompletedAtUtc = utcNow;
    }
}

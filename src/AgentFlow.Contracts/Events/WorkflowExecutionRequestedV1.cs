using AgentFlow.Contracts.Messaging;

namespace AgentFlow.Contracts.Events;

public sealed record WorkflowExecutionRequestedV1(
    Guid MessageId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid WorkflowId,
    Guid WorkflowVersionId,
    Guid ExecutionId) : IIntegrationEvent;

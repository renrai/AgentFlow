namespace AgentFlow.Contracts.Messaging;

public sealed record MessageEnvelope<TMessage>(
    Guid MessageId,
    string MessageType,
    string CorrelationId,
    string? CausationId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    TMessage Payload)
    where TMessage : class;

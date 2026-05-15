namespace AgentFlow.Contracts.Messaging;

public interface IMessage
{
    Guid MessageId { get; }

    DateTimeOffset OccurredAt { get; }
}

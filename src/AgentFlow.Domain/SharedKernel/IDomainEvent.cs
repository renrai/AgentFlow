namespace AgentFlow.Domain.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

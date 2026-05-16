using AgentFlow.Application.Abstractions.Clock;

namespace AgentFlow.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

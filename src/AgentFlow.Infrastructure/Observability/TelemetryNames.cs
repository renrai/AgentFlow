using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AgentFlow.Infrastructure.Observability;

public static class TelemetryNames
{
    public const string ActivitySourceName = "AgentFlow";

    public const string MeterName = "AgentFlow";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static readonly Meter Meter = new(MeterName);
}

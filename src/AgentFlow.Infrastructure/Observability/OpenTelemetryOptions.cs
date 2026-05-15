namespace AgentFlow.Infrastructure.Observability;

public sealed class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public string? ServiceName { get; init; }

    public string? OtlpEndpoint { get; init; }
}

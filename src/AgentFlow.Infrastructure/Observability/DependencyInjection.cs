using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AgentFlow.Infrastructure.Observability;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        bool includeAspNetCoreInstrumentation)
    {
        var section = configuration.GetSection(OpenTelemetryOptions.SectionName);
        var options = section.Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();
        var resolvedServiceName = string.IsNullOrWhiteSpace(options.ServiceName)
            ? serviceName
            : options.ServiceName;

        services.AddOptions<OpenTelemetryOptions>()
            .Bind(section)
            .ValidateOnStart();

        var builder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(resolvedServiceName));

        builder.WithTracing(tracing =>
        {
            tracing.AddSource(TelemetryNames.ActivitySourceName);
            tracing.AddHttpClientInstrumentation();

            if (includeAspNetCoreInstrumentation)
            {
                tracing.AddAspNetCoreInstrumentation();
            }

            if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
            {
                tracing.AddOtlpExporter(exporter =>
                    exporter.Endpoint = new Uri(options.OtlpEndpoint, UriKind.Absolute));
            }
        });

        builder.WithMetrics(metrics =>
        {
            metrics.AddMeter(TelemetryNames.MeterName);

            if (includeAspNetCoreInstrumentation)
            {
                metrics.AddAspNetCoreInstrumentation();
            }

            if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
            {
                metrics.AddOtlpExporter(exporter =>
                    exporter.Endpoint = new Uri(options.OtlpEndpoint, UriKind.Absolute));
            }
        });

        return services;
    }
}

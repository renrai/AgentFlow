using Microsoft.Extensions.Logging;

namespace AgentFlow.Infrastructure.Logging;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddPlatformJsonLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
            options.UseUtcTimestamp = true;
        });

        return logging;
    }
}

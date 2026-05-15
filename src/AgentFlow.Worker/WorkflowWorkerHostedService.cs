namespace AgentFlow.Worker;

public sealed class WorkflowWorkerHostedService(ILogger<WorkflowWorkerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogWorkerStarted(logger, null);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            LogWorkerStopped(logger, null);
        }
    }

    private static readonly Action<ILogger, Exception?> LogWorkerStarted =
        LoggerMessage.Define(LogLevel.Information, new EventId(1000, nameof(LogWorkerStarted)), "Workflow worker bootstrap started.");

    private static readonly Action<ILogger, Exception?> LogWorkerStopped =
        LoggerMessage.Define(LogLevel.Information, new EventId(1001, nameof(LogWorkerStopped)), "Workflow worker bootstrap stopped.");
}

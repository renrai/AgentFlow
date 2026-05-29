using System.Text;
using System.Text.Json;
using AgentFlow.Contracts.Events;
using AgentFlow.Infrastructure.Messaging;
using AgentFlow.Worker.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgentFlow.Worker.Consumers;

internal sealed class WorkflowExecutionConsumer(
    IRabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<WorkflowExecutionConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RabbitMqOptions _options = options.Value;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small startup delay so the topology initializer runs first.
        try { await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken).ConfigureAwait(false); }
        catch (OperationCanceledException) { return; }

        var connection = await connectionProvider.GetConnectionAsync(stoppingToken).ConfigureAwait(false);
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken).ConfigureAwait(false);

        // Process one message at a time per worker instance.
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken).ConfigureAwait(false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: _options.ExecutionQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken).ConfigureAwait(false);

        LogConsumerStarted(logger, _options.ExecutionQueueName, null);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            LogConsumerStopped(logger, null);
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (_channel is null) return;

        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.Span);
            var message = JsonSerializer.Deserialize<WorkflowExecutionRequestedV1>(json, JsonOptions);

            if (message is null)
            {
                LogDeserializationFailed(logger, ea.DeliveryTag, null);
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
                return;
            }

            LogMessageReceived(logger, message.ExecutionId, null);

            await using var scope = scopeFactory.CreateAsyncScope();
            var executor = scope.ServiceProvider.GetRequiredService<IWorkflowExecutor>();
            await executor.ExecuteAsync(message.ExecutionId, CancellationToken.None).ConfigureAwait(false);

            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogMessageProcessingFailed(logger, ea.DeliveryTag, ex);
            // Send to DLQ — Phase 6 will add retry policies.
            try
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
            }
            catch (Exception nackEx)
            {
                LogNackFailed(logger, ea.DeliveryTag, nackEx);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            try { await _channel.CloseAsync(cancellationToken).ConfigureAwait(false); } catch { /* ignored */ }
            await _channel.DisposeAsync().ConfigureAwait(false);
            _channel = null;
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    private static readonly Action<ILogger, string, Exception?> LogConsumerStarted =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(8000, nameof(LogConsumerStarted)),
            "Workflow execution consumer subscribed to queue '{Queue}'");

    private static readonly Action<ILogger, Exception?> LogConsumerStopped =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(8001, nameof(LogConsumerStopped)),
            "Workflow execution consumer stopped");

    private static readonly Action<ILogger, Guid, Exception?> LogMessageReceived =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(8002, nameof(LogMessageReceived)),
            "Workflow execution requested for {ExecutionId}");

    private static readonly Action<ILogger, ulong, Exception?> LogDeserializationFailed =
        LoggerMessage.Define<ulong>(
            LogLevel.Error,
            new EventId(8003, nameof(LogDeserializationFailed)),
            "Failed to deserialize message with delivery tag {DeliveryTag}");

    private static readonly Action<ILogger, ulong, Exception?> LogMessageProcessingFailed =
        LoggerMessage.Define<ulong>(
            LogLevel.Error,
            new EventId(8004, nameof(LogMessageProcessingFailed)),
            "Processing failed for message with delivery tag {DeliveryTag}");

    private static readonly Action<ILogger, ulong, Exception?> LogNackFailed =
        LoggerMessage.Define<ulong>(
            LogLevel.Error,
            new EventId(8005, nameof(LogNackFailed)),
            "Failed to nack message with delivery tag {DeliveryTag}");
}

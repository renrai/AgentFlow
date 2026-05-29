using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AgentFlow.Infrastructure.Messaging;

internal sealed class RabbitMqTopologyInitializer(
    IRabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqTopologyInitializer> logger) : IHostedService
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var connection = await connectionProvider.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            // Dead-letter exchange + queue
            await channel.ExchangeDeclareAsync(
                _options.DeadLetterExchangeName,
                ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await channel.QueueDeclareAsync(
                queue: _options.DeadLetterExchangeName + ".queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await channel.QueueBindAsync(
                queue: _options.DeadLetterExchangeName + ".queue",
                exchange: _options.DeadLetterExchangeName,
                routingKey: "#",
                cancellationToken: cancellationToken).ConfigureAwait(false);

            // Main exchange
            await channel.ExchangeDeclareAsync(
                _options.ExchangeName,
                ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            // Execution queue with DLX
            var arguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = _options.DeadLetterExchangeName,
                ["x-dead-letter-routing-key"] = RabbitMqTopologyNames.WorkflowDeadLetterRoutingKey
            };

            await channel.QueueDeclareAsync(
                queue: _options.ExecutionQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await channel.QueueBindAsync(
                queue: _options.ExecutionQueueName,
                exchange: _options.ExchangeName,
                routingKey: RabbitMqTopologyNames.WorkflowExecutionRequestedRoutingKey,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            LogTopologyReady(logger, _options.ExchangeName, _options.ExecutionQueueName, null);
        }
        catch (Exception ex)
        {
            LogTopologyFailed(logger, ex);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static readonly Action<ILogger, string, string, Exception?> LogTopologyReady =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(6010, nameof(LogTopologyReady)),
            "RabbitMQ topology ready: exchange '{Exchange}', queue '{Queue}'");

    private static readonly Action<ILogger, Exception?> LogTopologyFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(6011, nameof(LogTopologyFailed)),
            "Failed to initialize RabbitMQ topology");
}

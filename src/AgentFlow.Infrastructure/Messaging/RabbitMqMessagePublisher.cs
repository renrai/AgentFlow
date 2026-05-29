using System.Text.Json;
using AgentFlow.Application.Abstractions.Messaging;
using AgentFlow.Contracts.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AgentFlow.Infrastructure.Messaging;

internal sealed class RabbitMqMessagePublisher(
    IRabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqMessagePublisher> logger) : IMessagePublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RabbitMqOptions _options = options.Value;

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var routingKey = ResolveRoutingKey(message);
        var connection = await connectionProvider.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), JsonOptions);
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            Persistent = true,
            Type = message.GetType().Name,
            MessageId = Guid.NewGuid().ToString()
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        LogPublished(logger, message.GetType().Name, _options.ExchangeName, routingKey, null);
    }

    private static string ResolveRoutingKey<TMessage>(TMessage message)
        where TMessage : class
        => message switch
        {
            WorkflowExecutionRequestedV1 => RabbitMqTopologyNames.WorkflowExecutionRequestedRoutingKey,
            _ => throw new InvalidOperationException($"No routing key mapped for message type '{typeof(TMessage).Name}'.")
        };

    private static readonly Action<ILogger, string, string, string, Exception?> LogPublished =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Debug,
            new EventId(6001, nameof(LogPublished)),
            "Published {MessageType} to exchange '{Exchange}' with routing key '{RoutingKey}'");
}

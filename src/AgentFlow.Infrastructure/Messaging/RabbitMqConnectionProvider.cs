using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AgentFlow.Infrastructure.Messaging;

public interface IRabbitMqConnectionProvider : IAsyncDisposable
{
    ValueTask<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}

internal sealed class RabbitMqConnectionProvider(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConnectionProvider> logger) : IRabbitMqConnectionProvider
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public async ValueTask<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            if (_connection is not null)
            {
                try { await _connection.DisposeAsync().ConfigureAwait(false); } catch { /* ignored */ }
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            LogConnected(logger, _options.HostName, _options.Port, null);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            try { await _connection.DisposeAsync().ConfigureAwait(false); } catch { /* ignored */ }
            _connection = null;
        }

        _lock.Dispose();
    }

    private static readonly Action<ILogger, string, int, Exception?> LogConnected =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(6000, nameof(LogConnected)),
            "Connected to RabbitMQ at {Host}:{Port}");
}

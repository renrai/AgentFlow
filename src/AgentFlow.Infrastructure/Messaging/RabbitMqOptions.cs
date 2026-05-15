namespace AgentFlow.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string VirtualHost { get; init; } = "/";

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string ExchangeName { get; init; } = "agentflow.exchange";

    public string ExecutionQueueName { get; init; } = "agentflow.workflow-executions";

    public string DeadLetterExchangeName { get; init; } = "agentflow.dead-letter";
}

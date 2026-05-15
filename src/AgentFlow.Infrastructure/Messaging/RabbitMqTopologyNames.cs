namespace AgentFlow.Infrastructure.Messaging;

public static class RabbitMqTopologyNames
{
    public const string WorkflowExecutionRequestedRoutingKey = "workflow.execution.requested";

    public const string WorkflowStepScheduledRoutingKey = "workflow.step.scheduled";

    public const string WorkflowDeadLetterRoutingKey = "workflow.dead-letter";
}

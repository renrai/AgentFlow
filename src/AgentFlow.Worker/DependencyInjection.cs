using AgentFlow.Worker.Consumers;
using AgentFlow.Worker.Execution;
using AgentFlow.Worker.Execution.Nodes;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkflowExecution(this IServiceCollection services)
    {
        services.AddHttpClient("workflow-http");

        services.AddSingleton<INodeExecutor, NoopNodeExecutor>();
        services.AddSingleton<INodeExecutor, HttpRequestNodeExecutor>();
        services.AddSingleton<INodeExecutorRegistry, NodeExecutorRegistry>();

        services.AddScoped<IWorkflowExecutor, WorkflowExecutor>();

        services.AddHostedService<WorkflowExecutionConsumer>();

        return services;
    }
}

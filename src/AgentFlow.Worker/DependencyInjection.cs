using AgentFlow.Infrastructure.AiProviders;
using AgentFlow.Worker.Consumers;
using AgentFlow.Worker.Execution;
using AgentFlow.Worker.Execution.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkflowExecution(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("workflow-http");

        services.AddAiProviders(configuration);

        services.AddSingleton<INodeExecutor, NoopNodeExecutor>();
        services.AddSingleton<INodeExecutor, HttpRequestNodeExecutor>();
        services.AddSingleton<INodeExecutor, AiChatNodeExecutor>();
        services.AddSingleton<INodeExecutorRegistry, NodeExecutorRegistry>();

        services.AddScoped<IWorkflowExecutor, WorkflowExecutor>();

        services.AddHostedService<WorkflowExecutionConsumer>();

        return services;
    }
}

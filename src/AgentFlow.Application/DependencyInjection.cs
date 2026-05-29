using AgentFlow.Application.Executions.GetExecution;
using AgentFlow.Application.Executions.ListExecutions;
using AgentFlow.Application.Executions.StartExecution;
using AgentFlow.Application.Identity.GetCurrentUser;
using AgentFlow.Application.Identity.Login;
using AgentFlow.Application.Identity.Register;
using AgentFlow.Application.Tenants.CreateTenant;
using AgentFlow.Application.Tenants.GetMyTenants;
using AgentFlow.Application.Workflows.ArchiveWorkflow;
using AgentFlow.Application.Workflows.CreateWorkflow;
using AgentFlow.Application.Workflows.GetWorkflow;
using AgentFlow.Application.Workflows.ListWorkflows;
using AgentFlow.Application.Workflows.PublishWorkflow;
using AgentFlow.Application.Workflows.UpdateWorkflow;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers handlers usable by any host (API or Worker).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<CreateTenantHandler>();
        services.AddScoped<GetMyTenantsHandler>();

        services.AddScoped<CreateWorkflowHandler>();
        services.AddScoped<UpdateWorkflowHandler>();
        services.AddScoped<PublishWorkflowHandler>();
        services.AddScoped<ArchiveWorkflowHandler>();
        services.AddScoped<GetWorkflowHandler>();
        services.AddScoped<ListWorkflowsHandler>();

        services.AddScoped<StartExecutionHandler>();
        services.AddScoped<GetExecutionHandler>();
        services.AddScoped<ListExecutionsHandler>();

        return services;
    }

    /// <summary>
    /// Registers handlers that require identity infrastructure (password hashing, JWT issuance).
    /// Only call from hosts that expose authentication endpoints (e.g. the API).
    /// </summary>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();

        return services;
    }
}

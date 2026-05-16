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
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<CreateTenantHandler>();
        services.AddScoped<GetMyTenantsHandler>();

        services.AddScoped<CreateWorkflowHandler>();
        services.AddScoped<UpdateWorkflowHandler>();
        services.AddScoped<PublishWorkflowHandler>();
        services.AddScoped<ArchiveWorkflowHandler>();
        services.AddScoped<GetWorkflowHandler>();
        services.AddScoped<ListWorkflowsHandler>();

        return services;
    }
}

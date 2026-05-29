using AgentFlow.Application;
using AgentFlow.Infrastructure;
using AgentFlow.Infrastructure.Logging;
using AgentFlow.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddPlatformJsonLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Environment.ApplicationName,
    includeAspNetCoreInstrumentation: false);
builder.Services.AddWorkflowExecution();

var host = builder.Build();
await host.RunAsync();

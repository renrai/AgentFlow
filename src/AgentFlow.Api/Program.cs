using AgentFlow.Api.Auth;
using AgentFlow.Api.Endpoints;
using AgentFlow.Api.Middleware;
using AgentFlow.Api.Tenancy;
using AgentFlow.Application;
using AgentFlow.Application.Abstractions.Identity;
using AgentFlow.Application.Abstractions.Tenancy;
using AgentFlow.Infrastructure;
using AgentFlow.Infrastructure.Identity;
using AgentFlow.Infrastructure.Logging;
using AgentFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddPlatformJsonLogging();

builder.Services.AddApplication();
builder.Services.AddIdentityApplication();
builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Environment.ApplicationName,
    includeAspNetCoreInstrumentation: true);
builder.Services.AddPlatformIdentity(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AgentFlowDbContext>();
    await db.Database.MigrateAsync().ConfigureAwait(false);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var system = app.MapGroup("/system").WithTags("System").AllowAnonymous();

system.MapGet("/info", () => Results.Ok(new
{
    Service = "AgentFlow.Api",
    Environment = app.Environment.EnvironmentName,
    Status = "Ready"
}));

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapTenantEndpoints();
app.MapWorkflowEndpoints();
app.MapExecutionEndpoints();
app.MapWebhookEndpoints();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();

app.Run();

public partial class Program;

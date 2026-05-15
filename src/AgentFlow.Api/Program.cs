using AgentFlow.Api.Auth;
using AgentFlow.Application;
using AgentFlow.Infrastructure;
using AgentFlow.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddPlatformJsonLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Environment.ApplicationName,
    includeAspNetCoreInstrumentation: true);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();

app.Run();

public partial class Program;

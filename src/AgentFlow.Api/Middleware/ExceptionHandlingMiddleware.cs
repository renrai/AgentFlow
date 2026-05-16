using AgentFlow.Application.SharedKernel;
using AgentFlow.Domain.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace AgentFlow.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Validation Failed", ex.Message).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Domain Rule Violation", ex.Message).ConfigureAwait(false);
        }
        catch (AuthenticationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Authentication Failed", ex.Message).ConfigureAwait(false);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message).ConfigureAwait(false);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message).ConfigureAwait(false);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflict", ex.Message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUnhandledException(_logger, context.Request.Method, context.Request.Path, ex);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.").ConfigureAwait(false);
        }
    }

    private static readonly Action<ILogger, string, PathString, Exception> LogUnhandledException =
        LoggerMessage.Define<string, PathString>(
            LogLevel.Error,
            new EventId(5000, nameof(LogUnhandledException)),
            "Unhandled exception while processing request {Method} {Path}");

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem);
    }
}

using Microsoft.AspNetCore.Diagnostics;
using MythicNexus.Application.Errors;
using MythicNexus.Application.Users.Services;
using MythicNexus.Infrastructure.Middleware;

namespace MythicNexus.Api.Infrastructure.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString();
        switch (exception)
        {
            case DuplicateUserException dup:
                _logger.LogWarning(
                    dup,
                    "Registration conflict. Code={Code}. CorrelationId={CorrelationId}",
                    dup.Code,
                    correlationId);
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(
                    new { error = new { code = dup.Code, message = dup.PublicMessage } },
                    cancellationToken);
                return true;
        }

        _logger.LogError(
            exception,
            "Unhandled exception. CorrelationId={CorrelationId}",
            correlationId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        if (_env.IsDevelopment())
        {
            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    error = new
                    {
                        code = ErrorCodes.InternalServerError,
                        message = "An unexpected error occurred.",
                        detail = exception.Message,
                    },
                },
                cancellationToken);
        }
        else
        {
            await httpContext.Response.WriteAsJsonAsync(
                new { error = new { code = ErrorCodes.InternalServerError, message = "An unexpected error occurred." } },
                cancellationToken);
        }

        return true;
    }
}

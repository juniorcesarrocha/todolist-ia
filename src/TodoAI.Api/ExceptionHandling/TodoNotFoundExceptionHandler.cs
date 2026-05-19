using Microsoft.AspNetCore.Diagnostics;
using TodoAI.Application.Exceptions;

namespace TodoAI.Api.ExceptionHandling;

public sealed class TodoNotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<TodoNotFoundExceptionHandler> _logger;

    public TodoNotFoundExceptionHandler(ILogger<TodoNotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not TodoNotFoundException notFound)
        {
            return false;
        }

        _logger.LogWarning(notFound, "Todo not found.");
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(
            new { error = notFound.Message },
            cancellationToken);
        return true;
    }
}

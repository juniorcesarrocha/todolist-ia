using Microsoft.AspNetCore.Diagnostics;
using TodoAI.Application.Exceptions;

namespace TodoAI.Api.ExceptionHandling;

public sealed class TodoAlreadyCompletedExceptionHandler : IExceptionHandler
{
    private readonly ILogger<TodoAlreadyCompletedExceptionHandler> _logger;

    public TodoAlreadyCompletedExceptionHandler(ILogger<TodoAlreadyCompletedExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not TodoAlreadyCompletedException alreadyCompleted)
        {
            return false;
        }

        _logger.LogWarning(alreadyCompleted, "Todo already completed.");
        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await httpContext.Response.WriteAsJsonAsync(
            new { error = alreadyCompleted.Message },
            cancellationToken);
        return true;
    }
}

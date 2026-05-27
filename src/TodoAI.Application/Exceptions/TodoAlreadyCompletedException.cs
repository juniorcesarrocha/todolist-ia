namespace TodoAI.Application.Exceptions;

public sealed class TodoAlreadyCompletedException : Exception
{
    public TodoAlreadyCompletedException(Guid id)
        : base($"Todo with id '{id}' is already completed.")
    {
    }
}

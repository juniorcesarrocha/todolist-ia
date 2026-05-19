namespace TodoAI.Application.Exceptions;

public sealed class TodoNotFoundException : Exception
{
    public TodoNotFoundException(Guid id)
        : base($"Todo with id '{id}' was not found.")
    {
    }
}

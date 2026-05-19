using TodoAI.Domain.Enums;

namespace TodoAI.Application.Todos;

public static class TodoItemStatusParser
{
    public static bool TryParse(string value, out TodoItemStatus status)
    {
        if (!Enum.TryParse(value, ignoreCase: true, out status))
        {
            return false;
        }

        return Enum.IsDefined(status);
    }
}

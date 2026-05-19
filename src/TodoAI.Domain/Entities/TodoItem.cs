using TodoAI.Domain.Enums;
using TodoAI.Domain.Exceptions;

namespace TodoAI.Domain.Entities;

public sealed class TodoItem
{
    public const int TitleMaxLength = 200;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TodoItemStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private TodoItem()
    {
    }

    public static TodoItem Create(string title, string? description)
    {
        var trimmedTitle = title?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            throw new DomainException("Title is required.");
        }

        if (trimmedTitle.Length > TitleMaxLength)
        {
            throw new DomainException($"Title cannot exceed {TitleMaxLength} characters.");
        }

        var trimmedDescription = description?.Trim();

        return new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = trimmedTitle,
            Description = string.IsNullOrWhiteSpace(trimmedDescription) ? null : trimmedDescription,
            Status = TodoItemStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        if (Status == TodoItemStatus.Done)
        {
            return;
        }

        Status = TodoItemStatus.Done;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProgress()
    {
        if (Status == TodoItemStatus.InProgress)
        {
            return;
        }

        Status = TodoItemStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description)
    {
        var trimmedTitle = title?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            throw new DomainException("Title is required.");
        }

        if (trimmedTitle.Length > TitleMaxLength)
        {
            throw new DomainException($"Title cannot exceed {TitleMaxLength} characters.");
        }

        var trimmedDescription = description?.Trim();

        Title = trimmedTitle;
        Description = string.IsNullOrWhiteSpace(trimmedDescription) ? null : trimmedDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(TodoItemStatus status)
    {
        if (Status == status)
        {
            return;
        }

        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}

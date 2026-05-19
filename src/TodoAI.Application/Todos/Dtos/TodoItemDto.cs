using TodoAI.Domain.Enums;

namespace TodoAI.Application.Todos.Dtos;

public sealed record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    TodoItemStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

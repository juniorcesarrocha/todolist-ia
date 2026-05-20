using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Abstractions;

public interface ITodoItemRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> GetByStatusAsync(
        TodoItemStatus status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(TodoItemStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default);
}

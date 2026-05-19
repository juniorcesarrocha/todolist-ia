using Microsoft.EntityFrameworkCore;
using TodoAI.Application.Abstractions;
using TodoAI.Domain.Entities;
using TodoAI.Infrastructure.Persistence;

namespace TodoAI.Infrastructure.Repositories;

public sealed class TodoItemRepository : ITodoItemRepository
{
    private readonly TodoAiDbContext _dbContext;

    public TodoItemRepository(TodoAiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TodoItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TodoItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        await _dbContext.TodoItems.AddAsync(item, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.TodoItems.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.TodoItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

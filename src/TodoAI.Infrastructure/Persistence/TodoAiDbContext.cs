using Microsoft.EntityFrameworkCore;
using TodoAI.Domain.Entities;

namespace TodoAI.Infrastructure.Persistence;

public sealed class TodoAiDbContext : DbContext
{
    public TodoAiDbContext(DbContextOptions<TodoAiDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoAiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

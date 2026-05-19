using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;
using TodoAI.Infrastructure.Persistence;

namespace TodoAI.Infrastructure.Tests.Persistence;

public sealed class TodoAiDbContextTests
{
    [Fact]
    public async Task ShouldPersistTodoItem()
    {
        var options = new DbContextOptionsBuilder<TodoAiDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var context = new TodoAiDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var item = TodoItem.Create("Teste", "Descrição");
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        var stored = await context.TodoItems.SingleAsync();
        stored.Title.Should().Be("Teste");
        stored.Description.Should().Be("Descrição");
        stored.Status.Should().Be(TodoItemStatus.Pending);
    }
}

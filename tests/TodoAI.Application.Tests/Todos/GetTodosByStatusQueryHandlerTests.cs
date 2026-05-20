using FluentAssertions;
using Moq;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Todos.Queries.GetTodosByStatus;
using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Tests.Todos;

public sealed class GetTodosByStatusQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPagedResult()
    {
        var items = new List<TodoItem>
        {
            TodoItem.Create("Tarefa 1", null),
            TodoItem.Create("Tarefa 2", "Descricao")
        };

        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.CountByStatusAsync(TodoItemStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);
        repository
            .Setup(r => r.GetByStatusAsync(TodoItemStatus.Pending, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        var handler = new GetTodosByStatusQueryHandler(repository.Object);

        var result = await handler.Handle(
            new GetTodosByStatusQuery(TodoItemStatus.Pending, Page: 1, PageSize: 10),
            CancellationToken.None);

        result.Total.Should().Be(12);
        result.Page.Should().Be(1);
        result.TotalPages.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title).Should().Contain(new[] { "Tarefa 1", "Tarefa 2" });
    }

    [Fact]
    public async Task Handle_WhenPageHasNoItems_ShouldReturnEmptyPagedResult()
    {
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.CountByStatusAsync(TodoItemStatus.Done, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        repository
            .Setup(r => r.GetByStatusAsync(TodoItemStatus.Done, 10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TodoItem>());
        var handler = new GetTodosByStatusQueryHandler(repository.Object);

        var result = await handler.Handle(
            new GetTodosByStatusQuery(TodoItemStatus.Done, Page: 2, PageSize: 10),
            CancellationToken.None);

        result.Total.Should().Be(2);
        result.Page.Should().Be(2);
        result.TotalPages.Should().Be(1);
        result.Items.Should().BeEmpty();
    }
}

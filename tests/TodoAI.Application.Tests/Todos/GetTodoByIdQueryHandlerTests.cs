using FluentAssertions;
using Moq;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Queries.GetTodoById;
using TodoAI.Domain.Entities;

namespace TodoAI.Application.Tests.Todos;

public sealed class GetTodoByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenTodoExists_ShouldReturnDto()
    {
        var item = TodoItem.Create("Ler livro", "Capitulo 1");
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var handler = new GetTodoByIdQueryHandler(repository.Object);

        var result = await handler.Handle(new GetTodoByIdQuery(item.Id), CancellationToken.None);

        result.Id.Should().Be(item.Id);
        result.Title.Should().Be("Ler livro");
        result.Description.Should().Be("Capitulo 1");
    }

    [Fact]
    public async Task Handle_WhenTodoDoesNotExist_ShouldThrowTodoNotFoundException()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);
        var handler = new GetTodoByIdQueryHandler(repository.Object);

        var act = () => handler.Handle(new GetTodoByIdQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<TodoNotFoundException>()
            .WithMessage($"Todo with id '{id}' was not found.");
    }
}

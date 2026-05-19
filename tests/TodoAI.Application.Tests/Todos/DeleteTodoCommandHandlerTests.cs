using FluentAssertions;
using Moq;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Commands.DeleteTodo;
using TodoAI.Domain.Entities;

namespace TodoAI.Application.Tests.Todos;

public sealed class DeleteTodoCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTodoExists_ShouldDelete()
    {
        var item = TodoItem.Create("Remover", null);
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var handler = new DeleteTodoCommandHandler(repository.Object);

        await handler.Handle(new DeleteTodoCommand(item.Id), CancellationToken.None);

        repository.Verify(
            r => r.DeleteAsync(item, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTodoNotFound_ShouldThrowTodoNotFoundException()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);
        var handler = new DeleteTodoCommandHandler(repository.Object);

        var act = () => handler.Handle(new DeleteTodoCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<TodoNotFoundException>()
            .WithMessage($"Todo with id '{id}' was not found.");
        repository.Verify(
            r => r.DeleteAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

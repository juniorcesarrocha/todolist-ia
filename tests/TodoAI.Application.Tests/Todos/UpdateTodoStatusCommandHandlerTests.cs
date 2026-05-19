using FluentAssertions;
using Moq;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Commands.UpdateTodoStatus;
using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Tests.Todos;

public sealed class UpdateTodoStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTodoExists_ShouldUpdateStatusAndReturnDto()
    {
        var item = TodoItem.Create("Tarefa", null);
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var handler = new UpdateTodoStatusCommandHandler(repository.Object);

        var result = await handler.Handle(
            new UpdateTodoStatusCommand(item.Id, TodoItemStatus.InProgress),
            CancellationToken.None);

        result.Status.Should().Be(TodoItemStatus.InProgress);
        result.UpdatedAt.Should().NotBeNull();
        repository.Verify(
            r => r.UpdateAsync(item, It.IsAny<CancellationToken>()),
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
        var handler = new UpdateTodoStatusCommandHandler(repository.Object);

        var act = () => handler.Handle(
            new UpdateTodoStatusCommand(id, TodoItemStatus.Done),
            CancellationToken.None);

        await act.Should().ThrowAsync<TodoNotFoundException>()
            .WithMessage($"Todo with id '{id}' was not found.");
        repository.Verify(
            r => r.UpdateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

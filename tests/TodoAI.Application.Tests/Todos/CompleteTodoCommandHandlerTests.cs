using FluentAssertions;
using Moq;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Commands.CompleteTodo;
using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Tests.Todos;

public sealed class CompleteTodoCommandHandlerTests
{
    [Theory]
    [InlineData(TodoItemStatus.Pending)]
    [InlineData(TodoItemStatus.InProgress)]
    public async Task Handle_WhenTodoIsNotDone_ShouldCompleteAndReturnDto(TodoItemStatus initialStatus)
    {
        var item = TodoItem.Create("Tarefa", null);
        if (initialStatus == TodoItemStatus.InProgress)
        {
            item.StartProgress();
        }

        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var handler = new CompleteTodoCommandHandler(repository.Object);

        var result = await handler.Handle(
            new CompleteTodoCommand(item.Id),
            CancellationToken.None);

        result.Status.Should().Be(TodoItemStatus.Done);
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
        var handler = new CompleteTodoCommandHandler(repository.Object);

        var act = () => handler.Handle(new CompleteTodoCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<TodoNotFoundException>()
            .WithMessage($"Todo with id '{id}' was not found.");
        repository.Verify(
            r => r.UpdateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTodoAlreadyDone_ShouldThrowTodoAlreadyCompletedException()
    {
        var item = TodoItem.Create("Tarefa", null);
        item.Complete();
        var updatedAt = item.UpdatedAt;

        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var handler = new CompleteTodoCommandHandler(repository.Object);

        var act = () => handler.Handle(new CompleteTodoCommand(item.Id), CancellationToken.None);

        await act.Should().ThrowAsync<TodoAlreadyCompletedException>()
            .WithMessage($"Todo with id '{item.Id}' is already completed.");
        item.UpdatedAt.Should().Be(updatedAt);
        repository.Verify(
            r => r.UpdateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

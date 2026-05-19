using FluentAssertions;
using Moq;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Commands.UpdateTodo;
using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Tests.Todos;

public sealed class UpdateTodoCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTodoExists_ShouldUpdateAndReturnDto()
    {
        var item = TodoItem.Create("Título original", "Descrição original");
        var repository = new Mock<ITodoItemRepository>();
        repository
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        var handler = new UpdateTodoCommandHandler(repository.Object);

        var result = await handler.Handle(
            new UpdateTodoCommand(item.Id, "Novo título", "Nova descrição"),
            CancellationToken.None);

        result.Title.Should().Be("Novo título");
        result.Description.Should().Be("Nova descrição");
        result.Status.Should().Be(TodoItemStatus.Pending);
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
        var handler = new UpdateTodoCommandHandler(repository.Object);

        var act = () => handler.Handle(
            new UpdateTodoCommand(id, "Título", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<TodoNotFoundException>()
            .WithMessage($"Todo with id '{id}' was not found.");
        repository.Verify(
            r => r.UpdateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

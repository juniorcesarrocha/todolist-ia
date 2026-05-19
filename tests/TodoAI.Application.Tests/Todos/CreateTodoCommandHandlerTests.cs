using FluentAssertions;
using Moq;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Todos.Commands.CreateTodo;
using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Tests.Todos;

public sealed class CreateTodoCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPersistTodoAndReturnDto()
    {
        var repository = new Mock<ITodoItemRepository>();
        var handler = new CreateTodoCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateTodoCommand("Comprar leite"), CancellationToken.None);

        result.Title.Should().Be("Comprar leite");
        result.Status.Should().Be(TodoItemStatus.Pending);
        repository.Verify(
            r => r.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

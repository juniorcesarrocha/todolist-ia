using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Todos.Dtos;
using TodoAI.Domain.Entities;

namespace TodoAI.Application.Todos.Commands.CreateTodo;

public sealed class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, TodoItemDto>
{
    private readonly ITodoItemRepository _repository;

    public CreateTodoCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var item = TodoItem.Create(request.Title, description: null);
        await _repository.AddAsync(item, cancellationToken);

        return new TodoItemDto(
            item.Id,
            item.Title,
            item.Description,
            item.Status,
            item.CreatedAt,
            item.UpdatedAt);
    }
}

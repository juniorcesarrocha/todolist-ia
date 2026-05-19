using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Commands.UpdateTodo;

public sealed class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, TodoItemDto>
{
    private readonly ITodoItemRepository _repository;

    public UpdateTodoCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemDto> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            throw new TodoNotFoundException(request.Id);
        }

        item.UpdateDetails(request.Title, request.Description);
        await _repository.UpdateAsync(item, cancellationToken);

        return new TodoItemDto(
            item.Id,
            item.Title,
            item.Description,
            item.Status,
            item.CreatedAt,
            item.UpdatedAt);
    }
}

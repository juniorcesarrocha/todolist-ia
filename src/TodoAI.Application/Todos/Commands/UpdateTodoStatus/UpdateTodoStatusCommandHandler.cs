using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Commands.UpdateTodoStatus;

public sealed class UpdateTodoStatusCommandHandler : IRequestHandler<UpdateTodoStatusCommand, TodoItemDto>
{
    private readonly ITodoItemRepository _repository;

    public UpdateTodoStatusCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemDto> Handle(UpdateTodoStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            throw new TodoNotFoundException(request.Id);
        }

        item.ChangeStatus(request.Status);
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

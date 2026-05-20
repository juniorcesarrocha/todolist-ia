using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Queries.GetTodoById;

public sealed class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, TodoItemDto>
{
    private readonly ITodoItemRepository _repository;

    public GetTodoByIdQueryHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemDto> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            throw new TodoNotFoundException(request.Id);
        }

        return new TodoItemDto(
            item.Id,
            item.Title,
            item.Description,
            item.Status,
            item.CreatedAt,
            item.UpdatedAt);
    }
}

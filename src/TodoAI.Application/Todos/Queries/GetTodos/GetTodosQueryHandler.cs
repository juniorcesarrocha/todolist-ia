using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Queries.GetTodos;

public sealed class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, IReadOnlyList<TodoItemDto>>
{
    private readonly ITodoItemRepository _repository;

    public GetTodosQueryHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TodoItemDto>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);

        return items
            .Select(item => new TodoItemDto(
                item.Id,
                item.Title,
                item.Description,
                item.Status,
                item.CreatedAt,
                item.UpdatedAt))
            .ToList();
    }
}

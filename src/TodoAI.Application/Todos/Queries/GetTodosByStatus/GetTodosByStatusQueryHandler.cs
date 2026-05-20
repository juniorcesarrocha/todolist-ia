using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Common;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Queries.GetTodosByStatus;

public sealed class GetTodosByStatusQueryHandler : IRequestHandler<GetTodosByStatusQuery, PagedResult<TodoItemDto>>
{
    private readonly ITodoItemRepository _repository;

    public GetTodosByStatusQueryHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<TodoItemDto>> Handle(GetTodosByStatusQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var total = await _repository.CountByStatusAsync(request.Status, cancellationToken);
        var items = await _repository.GetByStatusAsync(request.Status, skip, pageSize, cancellationToken);

        var dtos = items
            .Select(item => new TodoItemDto(
                item.Id,
                item.Title,
                item.Description,
                item.Status,
                item.CreatedAt,
                item.UpdatedAt))
            .ToList();

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

        return new PagedResult<TodoItemDto>(dtos, total, page, totalPages);
    }
}

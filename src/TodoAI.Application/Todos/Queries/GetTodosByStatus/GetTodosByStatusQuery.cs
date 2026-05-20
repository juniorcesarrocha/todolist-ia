using MediatR;
using TodoAI.Application.Common;
using TodoAI.Application.Todos.Dtos;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Todos.Queries.GetTodosByStatus;

public sealed record GetTodosByStatusQuery(
    TodoItemStatus Status,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<TodoItemDto>>;

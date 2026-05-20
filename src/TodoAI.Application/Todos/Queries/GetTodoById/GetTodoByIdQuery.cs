using MediatR;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Queries.GetTodoById;

public sealed record GetTodoByIdQuery(Guid Id) : IRequest<TodoItemDto>;

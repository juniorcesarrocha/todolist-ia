using MediatR;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Queries.GetTodos;

public sealed record GetTodosQuery : IRequest<IReadOnlyList<TodoItemDto>>;

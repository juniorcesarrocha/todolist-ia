using MediatR;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Commands.UpdateTodo;

public sealed record UpdateTodoCommand(Guid Id, string Title, string? Description) : IRequest<TodoItemDto>;

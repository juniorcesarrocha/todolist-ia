using MediatR;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Commands.CompleteTodo;

public sealed record CompleteTodoCommand(Guid Id) : IRequest<TodoItemDto>;

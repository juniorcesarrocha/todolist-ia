using MediatR;
using TodoAI.Application.Todos.Dtos;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Todos.Commands.UpdateTodoStatus;

public sealed record UpdateTodoStatusCommand(Guid Id, TodoItemStatus Status) : IRequest<TodoItemDto>;

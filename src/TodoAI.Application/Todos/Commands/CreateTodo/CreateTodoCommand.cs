using MediatR;
using TodoAI.Application.Todos.Dtos;

namespace TodoAI.Application.Todos.Commands.CreateTodo;

public sealed record CreateTodoCommand(string Title) : IRequest<TodoItemDto>;

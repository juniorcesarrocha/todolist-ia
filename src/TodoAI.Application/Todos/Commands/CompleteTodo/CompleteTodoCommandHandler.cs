using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;
using TodoAI.Application.Todos.Dtos;
using TodoAI.Domain.Enums;

namespace TodoAI.Application.Todos.Commands.CompleteTodo;

public sealed class CompleteTodoCommandHandler : IRequestHandler<CompleteTodoCommand, TodoItemDto>
{
    private readonly ITodoItemRepository _repository;

    public CompleteTodoCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemDto> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            throw new TodoNotFoundException(request.Id);
        }

        if (item.Status == TodoItemStatus.Done)
        {
            throw new TodoAlreadyCompletedException(request.Id);
        }

        item.Complete();
        await _repository.UpdateAsync(item, cancellationToken);

        return new TodoItemDto(
            item.Id,
            item.Title,
            item.Description,
            item.Status,
            item.CreatedAt,
            item.UpdatedAt);
    }
}

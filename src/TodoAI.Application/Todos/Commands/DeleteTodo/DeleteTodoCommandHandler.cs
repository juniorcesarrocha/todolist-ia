using MediatR;
using TodoAI.Application.Abstractions;
using TodoAI.Application.Exceptions;

namespace TodoAI.Application.Todos.Commands.DeleteTodo;

public sealed class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand>
{
    private readonly ITodoItemRepository _repository;

    public DeleteTodoCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            throw new TodoNotFoundException(request.Id);
        }

        await _repository.DeleteAsync(item, cancellationToken);
    }
}

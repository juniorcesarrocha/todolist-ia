using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoAI.Application.Todos;
using TodoAI.Application.Todos.Commands.CreateTodo;
using TodoAI.Application.Todos.Commands.DeleteTodo;
using TodoAI.Application.Todos.Commands.UpdateTodo;
using TodoAI.Application.Todos.Commands.UpdateTodoStatus;
using TodoAI.Application.Todos.Queries.GetTodos;

namespace TodoAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TodosController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTodosQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTodoCommand(request.Title), cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateTodoCommand(id, request.Title, request.Description),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTodoCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateTodoStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TodoItemStatusParser.TryParse(request.Status, out var status))
        {
            return BadRequest(new { error = "Invalid status. Use Pending, InProgress, or Done." });
        }

        var result = await _mediator.Send(new UpdateTodoStatusCommand(id, status), cancellationToken);
        return Ok(result);
    }

    public sealed record CreateTodoRequest(string Title);

    public sealed record UpdateTodoRequest(string Title, string? Description);

    public sealed record UpdateTodoStatusRequest(string Status);
}

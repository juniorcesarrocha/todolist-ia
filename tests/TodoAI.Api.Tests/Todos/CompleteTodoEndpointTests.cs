using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoAI.Api.Tests.Infrastructure;
using TodoAI.Domain.Enums;

namespace TodoAI.Api.Tests.Todos;

public sealed class CompleteTodoEndpointTests : IClassFixture<TodoApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CompleteTodoEndpointTests(TodoApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PatchComplete_WhenTodoExists_ShouldReturn200WithDoneStatus()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/todos", new { title = "Concluir via PATCH" });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        created.Should().NotBeNull();

        var response = await _client.PatchAsync($"/api/todos/{created!.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TodoResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(created.Id);
        body.Status.Should().Be(TodoItemStatus.Done);
        body.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PatchComplete_WhenTodoNotFound_ShouldReturn404()
    {
        var id = Guid.NewGuid();

        var response = await _client.PatchAsync($"/api/todos/{id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task PatchComplete_WhenTodoAlreadyDone_ShouldReturn422()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/todos", new { title = "Já concluída" });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        created.Should().NotBeNull();

        var first = await _client.PatchAsync($"/api/todos/{created!.Id}/complete", null);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PatchAsync($"/api/todos/{created.Id}/complete", null);

        second.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await second.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().Contain(created.Id.ToString());
    }

    private sealed record TodoResponse(
        Guid Id,
        string Title,
        string? Description,
        TodoItemStatus Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    private sealed record ErrorResponse(string Error);
}

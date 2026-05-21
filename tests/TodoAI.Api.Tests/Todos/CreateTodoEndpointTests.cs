using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoAI.Api.Tests.Infrastructure;
using TodoAI.Domain.Enums;

namespace TodoAI.Api.Tests.Todos;

public sealed class CreateTodoEndpointTests : IClassFixture<TodoApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateTodoEndpointTests(TodoApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTodos_WithValidTitle_ShouldReturn201CreatedWithLocationAndBody()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/todos",
            new { title = "Nova tarefa via API" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<CreateTodoResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Title.Should().Be("Nova tarefa via API");
        body.Description.Should().BeNull();
        body.Status.Should().Be(TodoItemStatus.Pending);
        body.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        body.UpdatedAt.Should().BeNull();
    }

    private sealed record CreateTodoResponse(
        Guid Id,
        string Title,
        string? Description,
        TodoItemStatus Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}

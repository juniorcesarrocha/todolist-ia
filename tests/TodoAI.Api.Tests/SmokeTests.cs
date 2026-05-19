using FluentAssertions;

namespace TodoAI.Api.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void Solution_ShouldExposeApiAssembly()
    {
        typeof(TodoAI.Api.Controllers.TodosController).Assembly.GetName().Name
            .Should().Be("TodoAI.Api");
    }
}

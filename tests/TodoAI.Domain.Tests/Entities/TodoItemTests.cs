using FluentAssertions;
using TodoAI.Domain.Entities;
using TodoAI.Domain.Enums;
using TodoAI.Domain.Exceptions;

namespace TodoAI.Domain.Tests.Entities;

public sealed class TodoItemTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldInitializeAsPending()
    {
        var item = TodoItem.Create("  Estudar CQRS  ", "Descrição opcional");

        item.Id.Should().NotBeEmpty();
        item.Title.Should().Be("Estudar CQRS");
        item.Description.Should().Be("Descrição opcional");
        item.Status.Should().Be(TodoItemStatus.Pending);
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        item.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowDomainException()
    {
        var act = () => TodoItem.Create("   ", null);

        act.Should().Throw<DomainException>()
            .WithMessage("Title is required.");
    }

    [Fact]
    public void Create_WithTitleAboveMaxLength_ShouldThrowDomainException()
    {
        var act = () => TodoItem.Create(new string('a', TodoItem.TitleMaxLength + 1), null);

        act.Should().Throw<DomainException>()
            .WithMessage($"Title cannot exceed {TodoItem.TitleMaxLength} characters.");
    }

    [Fact]
    public void Complete_ShouldSetStatusToDoneAndUpdateTimestamp()
    {
        var item = TodoItem.Create("Tarefa", null);

        item.Complete();

        item.Status.Should().Be(TodoItemStatus.Done);
        item.UpdatedAt.Should().NotBeNull();
        item.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Complete_WhenAlreadyDone_ShouldRemainDoneWithoutUpdatingTimestamp()
    {
        var item = TodoItem.Create("Tarefa", null);
        item.Complete();
        var updatedAt = item.UpdatedAt;

        item.Complete();

        item.Status.Should().Be(TodoItemStatus.Done);
        item.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void StartProgress_ShouldSetStatusToInProgressAndUpdateTimestamp()
    {
        var item = TodoItem.Create("Tarefa", null);

        item.StartProgress();

        item.Status.Should().Be(TodoItemStatus.InProgress);
        item.UpdatedAt.Should().NotBeNull();
        item.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateDetails_WithValidTitle_ShouldUpdateTitleDescriptionAndTimestamp()
    {
        var item = TodoItem.Create("Título original", "Descrição original");

        item.UpdateDetails("  Novo título  ", "  Nova descrição  ");

        item.Title.Should().Be("Novo título");
        item.Description.Should().Be("Nova descrição");
        item.UpdatedAt.Should().NotBeNull();
        item.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ShouldThrowDomainException()
    {
        var item = TodoItem.Create("Tarefa", null);

        var act = () => item.UpdateDetails("   ", null);

        act.Should().Throw<DomainException>()
            .WithMessage("Title is required.");
    }

    [Fact]
    public void UpdateDetails_WithTitleAboveMaxLength_ShouldThrowDomainException()
    {
        var item = TodoItem.Create("Tarefa", null);

        var act = () => item.UpdateDetails(new string('a', TodoItem.TitleMaxLength + 1), null);

        act.Should().Throw<DomainException>()
            .WithMessage($"Title cannot exceed {TodoItem.TitleMaxLength} characters.");
    }

    [Fact]
    public void UpdateDetails_WithNullDescription_ShouldClearDescription()
    {
        var item = TodoItem.Create("Tarefa", "Descrição existente");

        item.UpdateDetails("Tarefa", null);

        item.Description.Should().BeNull();
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_WithWhitespaceDescription_ShouldClearDescription()
    {
        var item = TodoItem.Create("Tarefa", "Descrição existente");

        item.UpdateDetails("Tarefa", "   ");

        item.Description.Should().BeNull();
    }

    [Fact]
    public void ChangeStatus_WithDifferentStatus_ShouldUpdateStatus()
    {
        var item = TodoItem.Create("Tarefa", null);

        item.ChangeStatus(TodoItemStatus.InProgress);

        item.Status.Should().Be(TodoItemStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_WithDifferentStatus_ShouldUpdateTimestamp()
    {
        var item = TodoItem.Create("Tarefa", null);

        item.ChangeStatus(TodoItemStatus.Done);

        item.UpdatedAt.Should().NotBeNull();
        item.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ChangeStatus_WhenStatusAlreadyEqual_ShouldNotUpdateTimestamp()
    {
        var item = TodoItem.Create("Tarefa", null);
        item.ChangeStatus(TodoItemStatus.InProgress);
        var updatedAt = item.UpdatedAt;

        item.ChangeStatus(TodoItemStatus.InProgress);

        item.Status.Should().Be(TodoItemStatus.InProgress);
        item.UpdatedAt.Should().Be(updatedAt);
    }
}

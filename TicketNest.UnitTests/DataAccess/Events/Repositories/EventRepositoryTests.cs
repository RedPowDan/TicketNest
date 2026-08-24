using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.DbContext;
using TicketNest.DataAccess.Events.Implementations;
using TicketNest.Domain.Events.Filters;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Pagination;

namespace TicketNest.UnitTests.DataAccess.Events.Repositories;

[TestFixture]
public class EventRepositoryTests
{
    private static EventsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase($"EventRepositoryTests_{Guid.NewGuid()}")
            .Options;

        return new EventsDbContext(options);
    }

    private static Event CreateValidEvent(string title = "Test Event", int totalSeats = 100)
    {
        return Event.Create(title, null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), totalSeats).Value;
    }

    [Test]
    public async Task Save_Should_CreateNewEvent_When_EventDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateValidEvent();

        // Act
        await repository.Save(@event);

        // Assert
        var saved = await dbContext.Events.FindAsync(@event.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Event");
        saved.TotalSeats.Should().Be(100);
        saved.AvailableSeats.Should().Be(100);
    }

    [Test]
    public async Task Save_Should_UpdateExistingEvent_When_EventExists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateValidEvent("Original", 50);
        await repository.Save(@event);

        @event.ChangeTitle("Updated Title");
        @event.ChangeStartAtAndEndAt(DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4));

        // Act
        await repository.Save(@event);

        // Assert
        var updated = await dbContext.Events.FindAsync(@event.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.StartAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(3), precision: TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task Get_Should_ReturnEvent_When_Exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateValidEvent();
        await repository.Save(@event);

        // Act
        var result = await repository.Get(@event.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Event");
        result.TotalSeats.Should().Be(100);
        result.AvailableSeats.Should().Be(100);
    }

    [Test]
    public async Task Get_Should_ReturnNull_When_DoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        // Act
        var result = await repository.Get(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetAll_Should_ReturnPaginatedResults()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        for (var i = 0; i < 5; i++)
        {
            await repository.Save(CreateValidEvent($"Event {i}"));
        }

        var filter = new EventsFilter();
        var pagination = new PaginationRequest(page: 1, pageSize: 2);

        // Act
        var result = await repository.GetAll(filter, pagination);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.CurrentPage.Should().Be(1);
    }

    [Test]
    public async Task GetAll_Should_FilterByTitle()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        await repository.Save(CreateValidEvent("Conference"));
        await repository.Save(CreateValidEvent("Workshop"));

        var filter = new EventsFilter(title: "Conference");
        var pagination = new PaginationRequest(page: 1, pageSize: 10);

        // Act
        var result = await repository.GetAll(filter, pagination);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Conference");
    }

    [Test]
    public async Task GetAll_Should_FilterByDateRange()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        await repository.Save(CreateValidEvent("Early"));
        var lateEvent = Event.Create("Late", null, DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(11), totalSeats: 10).Value;
        await repository.Save(lateEvent);

        var filter = new EventsFilter(from: DateTime.UtcNow.AddDays(5), to: DateTime.UtcNow.AddDays(15));
        var pagination = new PaginationRequest(page: 1, pageSize: 10);

        // Act
        var result = await repository.GetAll(filter, pagination);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Late");
    }

    [Test]
    public async Task Remove_Should_ReturnTrue_When_EventExists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateValidEvent();
        await repository.Save(@event);

        // Act
        var result = await repository.Remove(@event.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await dbContext.Events.FindAsync(@event.Id);
        deleted.Should().BeNull();
    }

    [Test]
    public async Task Remove_Should_ReturnFalse_When_EventDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        // Act
        var result = await repository.Remove(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}

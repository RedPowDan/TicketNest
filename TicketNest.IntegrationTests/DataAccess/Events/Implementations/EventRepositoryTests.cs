using FluentAssertions;
using TicketNest.DataAccess.Events.Implementations;
using TicketNest.Domain.Filters;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Pagination;
using TicketNest.IntegrationTests.Infrastructure;

namespace TicketNest.IntegrationTests.DataAccess.Events.Implementations;

[Collection("Database")]
public class EventRepositoryTests : EventsPgDatabaseTestBase
{
    public EventRepositoryTests(PostgreSqlContainerFixture fixture) : base(fixture)
    {
    }

    #region Save

    [Fact]
    public async Task Save_should_create_new_event_when_event_does_not_exist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateTestEvent();

        // Act
        await repository.Save(@event);

        // Assert
        await using var assertContext = CreateDbContext();
        var saved = await assertContext.Events.FindAsync(@event.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Save_should_update_existing_event_when_event_exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateTestEvent("Original", 50);
        await repository.Save(@event);

        @event.ChangeTitle("Updated Title");
        @event.ChangeStartAtAndEndAt(DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4));

        // Act
        await repository.Save(@event);

        // Assert
        await using var assertContext = CreateDbContext();
        var updated = await assertContext.Events.FindAsync(@event.Id);
        updated.Should().NotBeNull();
        updated.Title.Should().Be("Updated Title");
        updated.TotalSeats.Should().Be(50);
        updated.AvailableSeats.Should().Be(50);
        updated.StartAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(3), TimeSpan.FromSeconds(1));
        updated.EndAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(4), TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Get

    [Fact]
    public async Task Get_should_return_event_when_event_exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateTestEvent("Concert", 200);
        await repository.Save(@event);

        // Act
        var result = await repository.Get(@event.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(@event.Id);
        result.Title.Should().Be("Concert");
        result.TotalSeats.Should().Be(200);
        result.AvailableSeats.Should().Be(200);
        result.StartAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromSeconds(1));
        result.EndAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(2), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Get_should_return_null_when_event_does_not_exist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        // Act
        var result = await repository.Get(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GetAll_should_return_paginated_results()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        for (var i = 0; i < 5; i++)
        {
            await repository.Save(CreateTestEvent($"Event {i}"));
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

    [Fact]
    public async Task GetAll_should_filter_by_title()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        await repository.Save(CreateTestEvent("Conference"));
        await repository.Save(CreateTestEvent("Workshop"));

        var filter = new EventsFilter(title: "Conference");
        var pagination = new PaginationRequest(page: 1, pageSize: 10);

        // Act
        var result = await repository.GetAll(filter, pagination);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Conference");
    }

    [Fact]
    public async Task GetAll_should_filter_by_date_range()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        await repository.Save(CreateTestEvent("Early"));
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

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_should_return_true_when_event_exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);
        var @event = CreateTestEvent();
        await repository.Save(@event);

        // Act
        var result = await repository.Remove(@event.Id);

        // Assert
        result.Should().BeTrue();
        await using var assertContext = CreateDbContext();
        var deleted = await assertContext.Events.FindAsync(@event.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Remove_should_return_false_when_event_does_not_exist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new EventRepository(dbContext);

        // Act
        var result = await repository.Remove(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    private static Event CreateTestEvent(string title = "Test Event", int totalSeats = 100)
    {
        return Event.Create(title, null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), totalSeats).Value;
    }
}
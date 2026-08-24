using FluentAssertions;
using NSubstitute;
using TicketNest.Application.Events.Services.Events;
using TicketNest.Domain.Events.Constants;
using TicketNest.Domain.Events.Filters;
using TicketNest.Domain.Events.Models;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Pagination;
using TicketNest.Domain.Events.Repositories;

namespace TicketNest.UnitTests.Application.Services.Events;

[TestFixture]
public class EventServiceTests
{
    private IEventsRepository _eventsRepository = null!;
    private EventService _eventService = null!;

    [SetUp]
    public void SetUp()
    {
        _eventsRepository = Substitute.For<IEventsRepository>();
        _eventService = new EventService(_eventsRepository);
    }

    #region Success Scenarios

    [Test]
    public async Task Create_Should_ReturnSuccess_When_ValidDataProvided()
    {
        const string title = "Valid Event Title";
        const string description = "Valid Description";
        var startAt = DateTime.UtcNow.AddDays(1);
        var endAt = DateTime.UtcNow.AddDays(2);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _eventService.Create(title, description, startAt, endAt, totalSeats: 100);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be(title);
        result.Value.Description.Should().Be(description);
        result.Value.StartAt.Should().Be(startAt);
        result.Value.EndAt.Should().Be(endAt);
        result.Value.TotalSeats.Should().Be(100);
        result.Value.AvailableSeats.Should().Be(100);

        await _eventsRepository.Received(1).Save(Arg.Is<Event>(e =>
            e.Title == title &&
            e.Description == description &&
            e.StartAt == startAt &&
            e.EndAt == endAt), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_ReturnPaginatedEvents_When_FilterAndPaginationProvided()
    {
        var filter = new EventsFilter();
        var paginationRequest = new PaginationRequest(page: 1, pageSize: 10);
        var expectedEvents = new List<Event>
        {
            CreateValidEvent(),
            CreateValidEvent()
        };
        var expectedResult = new PaginatedResult<Event>(expectedEvents, totalCount: 2, currentPage: 1);

        _eventsRepository.GetAll(filter, paginationRequest, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var result = await _eventService.GetAll(filter, paginationRequest);

        result.Should().NotBeNull();
        result.Items.Should().BeEquivalentTo(expectedResult.Items);
        result.TotalCount.Should().Be(expectedResult.TotalCount);
        result.CurrentPage.Should().Be(expectedResult.CurrentPage);
        result.Count.Should().Be(expectedResult.Count);

        await _eventsRepository.Received(1).GetAll(filter, paginationRequest, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Get_Should_ReturnEvent_When_ValidIdProvided()
    {
        var eventId = Guid.NewGuid();
        var expectedEvent = CreateValidEvent();

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(expectedEvent);

        var result = await _eventService.Get(eventId);

        result.Should().NotBeNull();
        result.Id.Should().Be(expectedEvent.Id);
        result.Title.Should().Be(expectedEvent.Title);

        await _eventsRepository.Received(1).Get(eventId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_UpdateSuccessfully_When_ValidDataProvided()
    {
        var eventId = Guid.NewGuid();
        var existingEvent = CreateValidEvent(eventId);
        const string newTitle = "Updated Title";
        const string newDescription = "Updated Description";
        var newStartAt = DateTime.UtcNow.AddDays(3);
        var newEndAt = DateTime.UtcNow.AddDays(4);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(existingEvent);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _eventService.Change(eventId, newTitle, newDescription, newStartAt, newEndAt);

        result.IsSuccess.Should().BeTrue();

        await _eventsRepository.Received(1).Save(Arg.Is<Event>(e =>
            e.Id == eventId &&
            e.Title == newTitle &&
            e.Description == newDescription &&
            e.StartAt == newStartAt &&
            e.EndAt == newEndAt), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Delete_Should_RemoveSuccessfully_When_ValidIdProvided()
    {
        var eventId = Guid.NewGuid();

        _eventsRepository.Remove(eventId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _eventService.Delete(eventId);

        result.IsSuccess.Should().BeTrue();

        await _eventsRepository.Received(1).Remove(eventId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_FilterByTitle_When_TitleProvided()
    {
        var filter = new EventsFilter(title: "Conference");
        var paginationRequest = new PaginationRequest(page: 1, pageSize: 10);
        var expectedEvents = new List<Event>
        {
            CreateValidEvent(title: "Tech Conference 2024"),
            CreateValidEvent(title: "Business Conference")
        };
        var expectedResult = new PaginatedResult<Event>(expectedEvents, 2, 1);

        _eventsRepository.GetAll(filter, paginationRequest, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var result = await _eventService.GetAll(filter, paginationRequest);

        result.Should().NotBeNull();
        result.Items.Should().AllSatisfy(e => e.Title.Should().Contain("Conference"));

        await _eventsRepository.Received(1).GetAll(
            Arg.Is<EventsFilter>(f => f.Title == "Conference"),
            paginationRequest,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_FilterByDateRange_When_StartDateAndEndDateProvided()
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var filter = new EventsFilter(from: startDate, to: endDate);
        var paginationRequest = new PaginationRequest(page: 1, pageSize: 10);
        var expectedEvents = new List<Event>
        {
            CreateValidEvent(startAt: DateTime.UtcNow.AddDays(5), endAt: DateTime.UtcNow.AddDays(6)),
            CreateValidEvent(startAt: DateTime.UtcNow.AddDays(10), endAt: DateTime.UtcNow.AddDays(11))
        };
        var expectedResult = new PaginatedResult<Event>(expectedEvents, 2, 1);

        _eventsRepository.GetAll(filter, paginationRequest, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var result = await _eventService.GetAll(filter, paginationRequest);

        result.Should().NotBeNull();
        result.Items.Should().AllSatisfy(e =>
        {
            e.StartAt.Should().BeOnOrAfter(startDate);
            e.EndAt.Should().BeOnOrBefore(endDate);
        });

        await _eventsRepository.Received(1).GetAll(
            Arg.Is<EventsFilter>(f => f.From == startDate && f.To == endDate),
            paginationRequest,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_ApplyCombinedFilters_When_MultipleFiltersProvided()
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var filter = new EventsFilter
        (
            title: "Workshop",
            from: startDate,
            to: endDate
        );
        var paginationRequest = new PaginationRequest(page: 1, pageSize: 10);
        var expectedEvents = new List<Event>
        {
            CreateValidEvent(
                title: "Python Workshop",
                startAt: DateTime.UtcNow.AddDays(5),
                endAt: DateTime.UtcNow.AddDays(6))
        };
        var expectedResult = new PaginatedResult<Event>(expectedEvents, 1, 1);

        _eventsRepository.GetAll(filter, paginationRequest, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var result = await _eventService.GetAll(filter, paginationRequest);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Contain("Workshop");
        result.Items.First().StartAt.Should().BeOnOrAfter(startDate);
        result.Items.First().EndAt.Should().BeOnOrBefore(endDate);

        await _eventsRepository.Received(1).GetAll(
            Arg.Is<EventsFilter>(f => f.Title == "Workshop" && f.From == startDate && f.To == endDate),
            paginationRequest,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Failure Scenarios

    [Test]
    public async Task Get_Should_ReturnNull_When_EventNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        _eventsRepository.Get(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Event?) null);

        var result = await _eventService.Get(nonExistentId);

        result.Should().BeNull();

        await _eventsRepository.Received(1).Get(nonExistentId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_ReturnNotFoundError_When_EventDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        _eventsRepository.Get(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Event?) null);

        var result = await _eventService.Change(
            nonExistentId,
            "New Title",
            "New Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Не найдено событие");

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnBadRequestError_When_InvalidDatesProvided()
    {
        const string title = "Invalid Event";
        const string description = "Description";
        var startAt = DateTime.UtcNow.AddDays(2);
        var endAt = DateTime.UtcNow.AddDays(1);

        var result = await _eventService.Create(title, description, startAt, endAt, totalSeats: 100);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        result.Error.Message.Should().NotBeNullOrEmpty();

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnBadRequestError_When_TitleIsEmpty()
    {
        const string title = "";
        const string description = "Description";
        var startAt = DateTime.UtcNow.AddDays(1);
        var endAt = DateTime.UtcNow.AddDays(2);

        var result = await _eventService.Create(title, description, startAt, endAt, totalSeats: 100);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnBadRequestError_When_TotalSeatsIsZero()
    {
        // Arrange
        const string title = "Valid Event";
        const string description = "Description";
        var startAt = DateTime.UtcNow.AddDays(1);
        var endAt = DateTime.UtcNow.AddDays(2);

        // Act
        var result = await _eventService.Create(title, description, startAt, endAt, totalSeats: 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnBadRequestError_When_TotalSeatsIsNegative()
    {
        // Arrange
        const string title = "Valid Event";
        const string description = "Description";
        var startAt = DateTime.UtcNow.AddDays(1);
        var endAt = DateTime.UtcNow.AddDays(2);

        // Act
        var result = await _eventService.Create(title, description, startAt, endAt, totalSeats: -1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_ReturnBadRequestError_When_EndDateIsBeforeStartDate()
    {
        var eventId = Guid.NewGuid();
        var existingEvent = CreateValidEvent(eventId);
        var newStartAt = DateTime.UtcNow.AddDays(5);
        var newEndAt = DateTime.UtcNow.AddDays(3);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(existingEvent);

        var result = await _eventService.Change(
            eventId,
            "Updated Title",
            "Updated Description",
            newStartAt,
            newEndAt);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_ReturnBadRequestError_When_TitleIsEmpty()
    {
        var eventId = Guid.NewGuid();
        var existingEvent = CreateValidEvent(eventId);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(existingEvent);

        var result = await _eventService.Change(
            eventId,
            "",
            "Updated Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Delete_Should_ReturnNotFoundError_When_EventDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        _eventsRepository.Remove(nonExistentId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _eventService.Delete(nonExistentId);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Событие не найдено.");

        await _eventsRepository.Received(1).Remove(nonExistentId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_ReturnEmptyPaginatedResult_When_NoEventsMatchFilter()
    {
        var filter = new EventsFilter(title: "NonExistentEvent");
        var paginationRequest = new PaginationRequest(page: 1, pageSize: 10);
        var expectedResult = new PaginatedResult<Event>(new List<Event>(), 0, 1);

        _eventsRepository.GetAll(filter, paginationRequest, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var result = await _eventService.GetAll(filter, paginationRequest);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private Event CreateValidEvent(
        Guid? id = null,
        string title = "Test Event",
        string? description = "Test Description",
        DateTime? startAt = null,
        DateTime? endAt = null)
    {
        var eventId = id ?? Guid.NewGuid();
        var start = startAt ?? DateTime.UtcNow.AddDays(1);
        var end = endAt ?? DateTime.UtcNow.AddDays(2);

        return Event.LoadFromStorage(eventId, title, description, start, end, totalSeats: 100, availableSeats: 100);
    }

    #endregion
}

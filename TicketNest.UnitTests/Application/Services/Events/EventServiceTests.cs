using FluentAssertions;
using NSubstitute;
using TicketNest.Application.Constants;
using TicketNest.Application.Services.Events;
using TicketNest.Domain.Filters;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Pagination;
using TicketNest.Domain.Repositories;

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
        // Arrange
        var title = "Valid Event Title";
        string? description = "Valid Description";
        var startAt = DateTime.UtcNow.AddDays(1);
        var endAt = DateTime.UtcNow.AddDays(2);
        var expectedEvent = Event.Create(title, description, startAt, endAt).Value;

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.Create(title, description, startAt, endAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be(title);
        result.Value.Description.Should().Be(description);
        result.Value.StartAt.Should().Be(startAt);
        result.Value.EndAt.Should().Be(endAt);
        
        await _eventsRepository.Received(1).Save(Arg.Is<Event>(e => 
            e.Title == title && 
            e.Description == description && 
            e.StartAt == startAt && 
            e.EndAt == endAt), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_ReturnPaginatedEvents_When_FilterAndPaginationProvided()
    {
        // Arrange
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

        // Act
        var result = await _eventService.GetAll(filter, paginationRequest);

        // Assert
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
        // Arrange
        var eventId = Guid.NewGuid();
        var expectedEvent = CreateValidEvent();
        
        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(expectedEvent);

        // Act
        var result = await _eventService.Get(eventId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedEvent.Id);
        result.Title.Should().Be(expectedEvent.Title);
        
        await _eventsRepository.Received(1).Get(eventId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_UpdateSuccessfully_When_ValidDataProvided()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = CreateValidEvent(eventId);
        var newTitle = "Updated Title";
        string? newDescription = "Updated Description";
        var newStartAt = DateTime.UtcNow.AddDays(3);
        var newEndAt = DateTime.UtcNow.AddDays(4);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(existingEvent);
        
        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.Change(eventId, newTitle, newDescription, newStartAt, newEndAt);

        // Assert
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
        // Arrange
        var eventId = Guid.NewGuid();
        
        _eventsRepository.Remove(eventId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _eventService.Delete(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        await _eventsRepository.Received(1).Remove(eventId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_FilterByTitle_When_TitleProvided()
    {
        // Arrange
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

        // Act
        var result = await _eventService.GetAll(filter, paginationRequest);

        // Assert
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
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var filter = new EventsFilter(from: startDate, to:endDate);
        var paginationRequest = new PaginationRequest(page: 1, pageSize: 10);
        var expectedEvents = new List<Event>
        {
            CreateValidEvent(startAt: DateTime.UtcNow.AddDays(5), endAt: DateTime.UtcNow.AddDays(6)),
            CreateValidEvent(startAt: DateTime.UtcNow.AddDays(10), endAt: DateTime.UtcNow.AddDays(11))
        };
        var expectedResult = new PaginatedResult<Event>(expectedEvents, 2, 1);

        _eventsRepository.GetAll(filter, paginationRequest, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _eventService.GetAll(filter, paginationRequest);

        // Assert
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
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var filter = new EventsFilter 
        ( 
            title:"Workshop",
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

        // Act
        var result = await _eventService.GetAll(filter, paginationRequest);

        // Assert
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
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        _eventsRepository.Get(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Event?)null);

        // Act
        var result = await _eventService.Get(nonExistentId);

        // Assert
        result.Should().BeNull();
        
        await _eventsRepository.Received(1).Get(nonExistentId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_ReturnNotFoundError_When_EventDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        _eventsRepository.Get(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Event?)null);

        // Act
        var result = await _eventService.Change(
            nonExistentId, 
            "New Title", 
            "New Description", 
            DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorStatusCode.NotFound);
        result.Error.Message.Should().Be("Не найдено событие");
        
        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnBadRequestError_When_InvalidDatesProvided()
    {
        // Arrange
        var title = "Invalid Event";
        string? description = "Description";
        var startAt = DateTime.UtcNow.AddDays(2);
        var endAt = DateTime.UtcNow.AddDays(1); // End date before start date

        // Act
        var result = await _eventService.Create(title, description, startAt, endAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorStatusCode.BadRequest);
        result.Error.Message.Should().NotBeNullOrEmpty();
        
        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnBadRequestError_When_TitleIsEmpty()
    {
        // Arrange
        var title = "";
        string? description = "Description";
        var startAt = DateTime.UtcNow.AddDays(1);
        var endAt = DateTime.UtcNow.AddDays(2);

        // Act
        var result = await _eventService.Create(title, description, startAt, endAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorStatusCode.BadRequest);
        
        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_ReturnBadRequestError_When_EndDateIsBeforeStartDate()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = CreateValidEvent(eventId);
        var newStartAt = DateTime.UtcNow.AddDays(5);
        var newEndAt = DateTime.UtcNow.AddDays(3); // End date before start date

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(existingEvent);

        // Act
        var result = await _eventService.Change(
            eventId, 
            "Updated Title", 
            "Updated Description", 
            newStartAt, 
            newEndAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorStatusCode.BadRequest);
        
        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Change_Should_ReturnBadRequestError_When_TitleIsEmpty()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = CreateValidEvent(eventId);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(existingEvent);

        // Act
        var result = await _eventService.Change(
            eventId, 
            "",
            "Updated Description", 
            DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorStatusCode.BadRequest);
        
        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Delete_Should_ReturnNotFoundError_When_EventDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        _eventsRepository.Remove(nonExistentId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _eventService.Delete(nonExistentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorStatusCode.NotFound);
        result.Error.Message.Should().Be("Событие не найдено.");
        
        await _eventsRepository.Received(1).Remove(nonExistentId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAll_Should_ReturnEmptyPaginatedResult_When_NoEventsMatchFilter()
    {
        // Arrange
        var filter = new EventsFilter  (title: "NonExistentEvent");
        var paginationRequest = new PaginationRequest(page: 1, pageSize: 10);
        var expectedResult = new PaginatedResult<Event>(new List<Event>(), 0, 1);

        _eventsRepository.GetAll(filter, paginationRequest, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _eventService.GetAll(filter, paginationRequest);

        // Assert
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
        
        return Event.LoadFromStorage(eventId, title, description, start, end);
    }

    #endregion
}
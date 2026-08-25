using NSubstitute;
using TicketNest.Domain.Events.Constants;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Repositories;
using TicketNest.Domain.Events.Services.Events;

namespace TicketNest.Events.Tests.Domain.Services.Events;

[TestFixture]
public class EventReleaseSeatsServiceTests
{
    private IEventsRepository _eventsRepository = null!;
    private EventReleaseSeatsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _eventsRepository = Substitute.For<IEventsRepository>();
        _service = new EventReleaseSeatsService(_eventsRepository);
    }

    [Test]
    public async Task ReleaseSeats_Should_ReleaseAndSave_When_EventExists()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var @event = Event.LoadFromStorage(
            id: eventId,
            title: "Test",
            description: "Test",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10,
            availableSeats: 5);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);
        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ReleaseSeats(eventId, ct: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        @event.AvailableSeats.Should().Be(6);
        await _eventsRepository.Received(1).Save(@event, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_WithCount_Should_ReleaseRequestedNumberOfSeats()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var @event = Event.LoadFromStorage(
            id: eventId,
            title: "Test",
            description: "Test",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10,
            availableSeats: 5);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);
        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ReleaseSeats(eventId, count: 2, ct: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        @event.AvailableSeats.Should().Be(7);
        await _eventsRepository.Received(1).Save(@event, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_Should_ReturnNotFound_When_EventDoesNotExist()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns((Event?)null);

        // Act
        var result = await _service.ReleaseSeats(eventId, ct: CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Contain(eventId.ToString());

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_Should_ReturnBadRequest_When_SeatsCannotBeReleased()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var @event = Event.LoadFromStorage(
            id: eventId,
            title: "Test",
            description: "Test",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10,
            availableSeats: 10);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);
        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ReleaseSeats(eventId, ct: CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        result.Error.Message.Should().Be("Невозможно вернуть бронь");

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_Should_ReturnBadRequest_When_RepositoryThrows()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<Event?>(new InvalidOperationException("db error")));

        // Act
        var result = await _service.ReleaseSeats(eventId, ct: CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        result.Error.Message.Should().Be("Неизвестная ошибка отмены брони");
    }
}

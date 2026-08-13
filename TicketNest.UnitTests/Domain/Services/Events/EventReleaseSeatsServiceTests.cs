using FluentAssertions;
using NSubstitute;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Events;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Domain.Services.Events;

[TestFixture]
public class EventReleaseSeatsServiceTests
{
    private IBookingRepository _bookingRepository = null!;
    private IEventsRepository _eventsRepository = null!;
    private EventReleaseSeatsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = Substitute.For<IBookingRepository>();
        _eventsRepository = Substitute.For<IEventsRepository>();
        _service = new EventReleaseSeatsService(_eventsRepository, _bookingRepository);
    }

    [Test]
    public async Task ReleaseSeats_WithBookingId_Should_ReleaseAndSave_When_BookingAndEventExist()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var booking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: eventId,
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);
        var @event = Event.LoadFromStorage(
            id: eventId,
            title: "Test",
            description: "Test",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10,
            availableSeats: 5);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);
        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);
        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ReleaseSeats(bookingId, ct: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        @event.AvailableSeats.Should().Be(6);
        await _eventsRepository.Received(1).Save(@event, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_WithCount_Should_ReleaseRequestedNumberOfSeats()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var booking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: eventId,
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);
        var @event = Event.LoadFromStorage(
            id: eventId,
            title: "Test",
            description: "Test",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10,
            availableSeats: 5);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);
        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);
        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ReleaseSeats(bookingId, count: 2, ct: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        @event.AvailableSeats.Should().Be(7);
        await _eventsRepository.Received(1).Save(@event, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_WithBookingId_Should_ReturnNotFound_When_BookingDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns((Booking?)null);

        // Act
        var result = await _service.ReleaseSeats(bookingId, ct: CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Contain(bookingId.ToString());

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_WithBookingId_Should_ReturnNotFound_When_EventDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var booking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: eventId,
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);
        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns((Event?)null);

        // Act
        var result = await _service.ReleaseSeats(bookingId, ct: CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Contain(eventId.ToString());

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_WithBookingId_Should_ReturnBadRequest_When_SeatsCannotBeReleased()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var booking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: eventId,
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);
        var @event = Event.LoadFromStorage(
            id: eventId,
            title: "Test",
            description: "Test",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10,
            availableSeats: 10);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);
        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);
        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ReleaseSeats(bookingId, ct: CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        result.Error.Message.Should().Be("Невозможно вернуть бронь");

        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReleaseSeats_WithBookingId_Should_ReturnBadRequest_When_RepositoryThrows()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var booking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: eventId,
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);
        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<Event?>(new InvalidOperationException("db error")));

        // Act
        var result = await _service.ReleaseSeats(bookingId, ct: CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        result.Error.Message.Should().Be("Неизвестная ошибка отмены брони");
    }

    [Test]
    public async Task ReleaseSeats_WithEmptyBookingId_Should_Throw()
    {
        // Arrange
        var bookingId = Guid.Empty;

        // Act & Assert
        var act = () => _service.ReleaseSeats(bookingId, ct: CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}

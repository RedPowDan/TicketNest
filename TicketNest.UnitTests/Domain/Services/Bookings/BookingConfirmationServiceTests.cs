using FluentAssertions;
using NSubstitute;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Domain.Services.Bookings;

[TestFixture]
public class BookingConfirmationServiceTests
{
    private IBookingRepository _bookingRepository = null!;
    private IEventsRepository _eventsRepository = null!;
    private BookingConfirmationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = Substitute.For<IBookingRepository>();
        _eventsRepository = Substitute.For<IEventsRepository>();
        _service = new BookingConfirmationService(_bookingRepository, _eventsRepository);
    }

    [Test]
    public async Task Confirm_WithBooking_Should_SetStatusToConfirmed()
    {
        // Arrange
        var booking = Booking.LoadFromStorage(
            id: Guid.CreateVersion7(),
            eventId: Guid.CreateVersion7(),
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);

        // Act
        var result = await _service.Confirm(booking, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ProcessedAt.Should().NotBeNull();
        booking.ProcessedAt.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test]
    public async Task Confirm_WithBooking_Should_SetProcessedAt_AfterCreatedAt()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddHours(-1);
        var booking = Booking.LoadFromStorage(
            id: Guid.CreateVersion7(),
            eventId: Guid.CreateVersion7(),
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: createdAt,
            processedAt: null);

        // Act
        var result = await _service.Confirm(booking, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.ProcessedAt.Should().BeAfter(createdAt);
    }

    [Test]
    public async Task Confirm_WithBookingId_Should_ConfirmAndSave_When_BookingAndEventExist()
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

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Confirm(bookingId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        await _bookingRepository.Received(1).Save(booking, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Confirm_WithBookingId_Should_ReturnNotFound_When_BookingDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns((Booking?) null);

        // Act
        var result = await _service.Confirm(bookingId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Contain(bookingId.ToString());

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Confirm_WithBookingId_Should_ReturnNotFound_When_EventDoesNotExist()
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
            .Returns((Event?) null);

        // Act
        var result = await _service.Confirm(bookingId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Contain(eventId.ToString());

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }
}

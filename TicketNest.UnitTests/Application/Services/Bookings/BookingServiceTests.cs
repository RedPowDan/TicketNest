using FluentAssertions;
using NSubstitute;
using TicketNest.Application.Bookings.Services.Bookings;
using TicketNest.Domain.Bookings.Constants;
using TicketNest.Domain.Bookings.Services.Bookings;
using TicketNest.Domain.Bookings.Models;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Repositories;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Application.Services.Bookings;

[TestFixture]
public class BookingServiceTests
{
    private IBookingFactory _bookingFactory = null!;
    private IBookingRepository _bookingRepository = null!;
    private BookingService _bookingService = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingFactory = Substitute.For<IBookingFactory>();
        _bookingRepository = Substitute.For<IBookingRepository>();
        _bookingService = new BookingService(_bookingFactory, _bookingRepository);
    }

    #region Create

    [Test]
    public async Task Create_Should_ReturnBooking_When_FactorySucceeds()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, userId: userId, status: BookingStatus.Pending);

        _bookingFactory.Create(eventId, userId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        _bookingRepository.GetBookingsByUserId(userId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Booking>());

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _bookingService.Create(eventId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(BookingStatus.Pending);
        result.Value.EventId.Should().Be(eventId);
        result.Value.UserId.Should().Be(userId);

        await _bookingRepository.Received(1).Save(Arg.Is<Booking>(b => b.Id == booking.Id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnFactoryError_When_FactoryFails()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var error = new Error(ErrorCode.NotFound, "Событие не найдено");

        _bookingFactory.Create(eventId, userId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromFailure(error));

        // Act
        var result = await _bookingService.Create(eventId, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Событие не найдено");

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnConflict_When_UserReachedBookingLimit()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, userId: userId, status: BookingStatus.Pending);

        _bookingFactory.Create(eventId, userId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        var existingBookings = Enumerable.Range(0, 10)
            .Select(_ => CreateValidBooking(eventId: eventId, userId: userId))
            .ToArray();

        _bookingRepository.GetBookingsByUserId(userId, Arg.Any<CancellationToken>())
            .Returns(existingBookings);

        // Act
        var result = await _bookingService.Create(eventId, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.Conflict);
        result.Error.Message.Should().Contain("активных броней");

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_AllowOtherUser_When_OneUserAtLimit()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var limitedUserId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, userId: otherUserId, status: BookingStatus.Pending);

        _bookingFactory.Create(eventId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        var limitedUserExisting = Enumerable.Range(0, 10)
            .Select(_ => CreateValidBooking(eventId: eventId, userId: limitedUserId))
            .ToArray();

        _bookingRepository.GetBookingsByUserId(limitedUserId, Arg.Any<CancellationToken>())
            .Returns(limitedUserExisting);
        _bookingRepository.GetBookingsByUserId(otherUserId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Booking>());

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var limitedResult = await _bookingService.Create(eventId, limitedUserId);
        var otherResult = await _bookingService.Create(eventId, otherUserId);

        // Assert
        limitedResult.IsFailure.Should().BeTrue();
        limitedResult.Error.StatusCode.Should().Be(ErrorCode.Conflict);

        otherResult.IsSuccess.Should().BeTrue();

        await _bookingRepository.Received(1).Save(Arg.Is<Booking>(b => b.UserId == otherUserId), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Get

    [Test]
    public async Task Get_Should_ReturnBooking_When_Exists()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var booking = CreateValidBooking(bookingId, eventId);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);

        // Act
        var result = await _bookingService.Get(bookingId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(bookingId);
        result.Value.EventId.Should().Be(eventId);
        result.Value.Status.Should().Be(BookingStatus.Pending);
    }

    [Test]
    public async Task Get_Should_ReturnNotFound_When_Missing()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        _bookingRepository.Get(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Booking?)null);

        // Act
        var result = await _bookingService.Get(nonExistentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Бронирование не найдено");
    }

    #endregion

    #region Booking Status Change Tests

    [Test]
    public void Confirm_Should_SetStatusToConfirmed_And_ProcessedAt()
    {
        // Arrange
        var booking = CreateValidBooking(status: BookingStatus.Pending);
        var processedAt = DateTime.UtcNow;

        // Act
        booking.Confirm(processedAt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ProcessedAt.Should().Be(processedAt);
    }

    [Test]
    public void Reject_Should_SetStatusToRejected_And_ProcessedAt()
    {
        // Arrange
        var booking = CreateValidBooking(status: BookingStatus.Pending);
        var processedAt = DateTime.UtcNow;

        // Act
        booking.Reject(processedAt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        booking.ProcessedAt.Should().Be(processedAt);
    }

    [Test]
    public void Reject_And_ReleaseSeats_Should_RestoreAvailableSeats()
    {
        // Arrange
        var @event = CreateTestEvent(totalSeats: 5, availableSeats: 4);
        var booking = CreateValidBooking(eventId: @event.Id, status: BookingStatus.Pending);

        // Act
        booking.Reject(DateTime.UtcNow);
        @event.ReleaseSeats();

        // Assert
        @event.AvailableSeats.Should().Be(5);
        booking.Status.Should().Be(BookingStatus.Rejected);
    }

    #endregion

    #region Helper Methods

    private static Booking CreateValidBooking(
        Guid? id = null,
        Guid? eventId = null,
        Guid? userId = null,
        BookingStatus status = BookingStatus.Pending)
    {
        return Booking.LoadFromStorage(
            id: id ?? Guid.CreateVersion7(),
            eventId: eventId ?? Guid.CreateVersion7(),
            userId: userId ?? Guid.CreateVersion7(),
            status: status,
            createdAt: DateTime.UtcNow,
            processedAt: null);
    }

    private static Event CreateTestEvent(Guid? id = null, int totalSeats = 10, int? availableSeats = null)
    {
        var eventId = id ?? Guid.CreateVersion7();
        return Event.LoadFromStorage(
            id: eventId,
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: totalSeats,
            availableSeats: availableSeats ?? totalSeats);
    }

    #endregion
}

using FluentAssertions;
using NSubstitute;
using TicketNest.Application.Services.Bookings;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Application.Services.Bookings;

[TestFixture]
public class BookingServiceTests
{
    private IBookingFactory _bookingFactory = null!;
    private IBookingRepository _bookingRepository = null!;
    private IQueueMessageRepository _queueMessageRepository = null!;
    private IBookingService _bookingService = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingFactory = Substitute.For<IBookingFactory>();
        _bookingRepository = Substitute.For<IBookingRepository>();
        _queueMessageRepository = Substitute.For<IQueueMessageRepository>();
        _bookingService = new BookingService(_bookingFactory, _bookingRepository, _queueMessageRepository);
    }

    #region Success Scenarios

    [Test]
    public async Task Create_Should_ReturnPending_When_EventExists()
    {
        var eventId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _bookingService.Create(eventId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(BookingStatus.Pending);
        result.Value.EventId.Should().Be(eventId);

        await _bookingRepository.Received(1).Save(Arg.Is<Booking>(b =>
            b.Id == booking.Id &&
            b.EventId == eventId &&
            b.Status == BookingStatus.Pending), Arg.Any<CancellationToken>());

        await _queueMessageRepository.Received(1).Create(
            Arg.Is<QueueMessage<BookingCreatedMessage>>(m =>
                m.Data.BookingId == booking.Id &&
                m.QueueName == QueueNames.BookingQueue),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_GenerateUniqueIds_ForMultipleBookings()
    {
        var eventId = Guid.CreateVersion7();
        var booking1 = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);
        var booking2 = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(
                Result<Booking, Error>.FromSuccess(booking1),
                Result<Booking, Error>.FromSuccess(booking2));

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result1 = await _bookingService.Create(eventId);
        var result2 = await _bookingService.Create(eventId);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }

    [Test]
    public async Task Get_Should_ReturnBooking_When_Exists()
    {
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var booking = CreateValidBooking(bookingId, eventId, BookingStatus.Pending);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);

        var result = await _bookingService.Get(bookingId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(bookingId);
        result.Value.EventId.Should().Be(eventId);
        result.Value.Status.Should().Be(BookingStatus.Pending);
    }

    [Test]
    public async Task Get_Should_ReflectStatusChange_AfterConfirm()
    {
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();

        var pendingBooking = CreateValidBooking(bookingId, eventId, BookingStatus.Pending);
        var confirmedBooking = CreateValidBooking(bookingId, eventId, BookingStatus.Confirmed);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(pendingBooking);

        var pendingResult = await _bookingService.Get(bookingId);
        pendingResult.IsSuccess.Should().BeTrue();
        pendingResult.Value.Status.Should().Be(BookingStatus.Pending);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(confirmedBooking);

        var confirmedResult = await _bookingService.Get(bookingId);

        confirmedResult.IsSuccess.Should().BeTrue();
        confirmedResult.Value.Status.Should().Be(BookingStatus.Confirmed);
    }

    #endregion

    #region Failure Scenarios

    [Test]
    public async Task Create_Should_ReturnNotFound_When_EventDoesNotExist()
    {
        var eventId = Guid.CreateVersion7();
        var error = new Error(ErrorCode.NotFound, "Событие не найдено");

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromFailure(error));

        var result = await _bookingService.Create(eventId);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Событие не найдено");

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _queueMessageRepository.DidNotReceive().Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnNotFound_When_EventDeleted()
    {
        var eventId = Guid.CreateVersion7();
        var error = new Error(ErrorCode.NotFound, "Событие не найдено");

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromFailure(error));

        var result = await _bookingService.Create(eventId);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Событие не найдено");

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _queueMessageRepository.DidNotReceive().Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Get_Should_ReturnNotFound_When_BookingDoesNotExist()
    {
        var nonExistentId = Guid.CreateVersion7();

        _bookingRepository.Get(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Booking?) null);

        var result = await _bookingService.Get(nonExistentId);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Бронирование не найдено");
    }

    #endregion

    #region Helper Methods

    private static Booking CreateValidBooking(
        Guid? id = null,
        Guid? eventId = null,
        BookingStatus status = BookingStatus.Pending)
    {
        return Booking.LoadFromStorage(
            id: id ?? Guid.CreateVersion7(),
            eventId: eventId ?? Guid.CreateVersion7(),
            status: status,
            createdAt: DateTime.UtcNow,
            processedAt: null);
    }

    #endregion
}

using FluentAssertions;
using NSubstitute;
using TicketNest.Application.Services.Bookings;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Events;
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
    private IEventsRepository _eventsRepository = null!;
    private IBookingService _bookingService = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingFactory = Substitute.For<IBookingFactory>();
        _bookingRepository = Substitute.For<IBookingRepository>();
        _queueMessageRepository = Substitute.For<IQueueMessageRepository>();
        _eventsRepository = Substitute.For<IEventsRepository>();
        _bookingService = new BookingService(_bookingFactory, _bookingRepository, _queueMessageRepository, _eventsRepository);
    }

    #region Success Scenarios

    [Test]
    public async Task Create_Should_ReturnPending_When_EventExists()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);
        var @event = CreateTestEvent(eventId, totalSeats: 10);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _bookingService.Create(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(BookingStatus.Pending);
        result.Value.EventId.Should().Be(eventId);

        await _bookingRepository.Received(1).Save(Arg.Is<Booking>(b =>
            b.Id == booking.Id &&
            b.EventId == eventId &&
            b.Status == BookingStatus.Pending), Arg.Any<CancellationToken>());

        await _eventsRepository.Received(1).Save(Arg.Is<Event>(e =>
            e.Id == eventId &&
            e.AvailableSeats == 9), Arg.Any<CancellationToken>());

        await _queueMessageRepository.Received(1).Create(
            Arg.Is<QueueMessage<BookingCreatedMessage>>(m =>
                m.Data.BookingId == booking.Id &&
                m.QueueName == QueueNames.BookingQueue),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_DecreaseAvailableSeats_ByOne_When_BookingCreated()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);
        var @event = CreateTestEvent(eventId, totalSeats: 5, availableSeats: 5);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _bookingService.Create(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        @event.AvailableSeats.Should().Be(4);

        await _eventsRepository.Received(1).Save(Arg.Is<Event>(e =>
            e.AvailableSeats == 4), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_Succeed_ForMultipleBookings_UntilLimitReached()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var @event = CreateTestEvent(eventId, totalSeats: 3);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(_ => Result<Booking, Error>.FromSuccess(
                CreateValidBooking(eventId: eventId)));

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _bookingService.Create(eventId);
        var result2 = await _bookingService.Create(eventId);
        var result3 = await _bookingService.Create(eventId);
        var result4 = await _bookingService.Create(eventId);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();
        result4.IsFailure.Should().BeTrue();
        result4.Error.StatusCode.Should().Be(ErrorCode.Conflict);
        result4.Error.Message.Should().Be("No available seats for this event");

        result1.Value.Id.Should().NotBe(result2.Value.Id);
        result2.Value.Id.Should().NotBe(result3.Value.Id);
    }

    [Test]
    public async Task Create_Should_GenerateUniqueIds_ForMultipleBookings()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var booking1 = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);
        var booking2 = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);
        var @event = CreateTestEvent(eventId, totalSeats: 10);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(
                Result<Booking, Error>.FromSuccess(booking1),
                Result<Booking, Error>.FromSuccess(booking2));

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _bookingService.Create(eventId);
        var result2 = await _bookingService.Create(eventId);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }

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
    public async Task Get_Should_ReflectStatusChange_AfterConfirm()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();

        var pendingBooking = CreateValidBooking(bookingId, eventId);
        var confirmedBooking = CreateValidBooking(bookingId, eventId, BookingStatus.Confirmed);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(pendingBooking);

        // Act
        var pendingResult = await _bookingService.Get(bookingId);
        pendingResult.IsSuccess.Should().BeTrue();
        pendingResult.Value.Status.Should().Be(BookingStatus.Pending);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(confirmedBooking);

        var confirmedResult = await _bookingService.Get(bookingId);

        // Assert
        confirmedResult.IsSuccess.Should().BeTrue();
        confirmedResult.Value.Status.Should().Be(BookingStatus.Confirmed);
    }

    #endregion

    #region Failure Scenarios

    [Test]
    public async Task Create_Should_ReturnNotFound_When_EventDoesNotExist()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var error = new Error(ErrorCode.NotFound, "Событие не найдено");

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromFailure(error));

        // Act
        var result = await _bookingService.Create(eventId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Событие не найдено");

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _queueMessageRepository.DidNotReceive().Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnNotFound_When_EventDeletedDuringBooking()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns((Event?) null);

        // Act
        var result = await _bookingService.Create(eventId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("Событие не найдено");

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _queueMessageRepository.DidNotReceive().Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Should_ReturnConflict_When_NoAvailableSeats()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var booking = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);
        var @event = CreateTestEvent(eventId, totalSeats: 5, availableSeats: 0);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(booking));

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        // Act
        var result = await _bookingService.Create(eventId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.Conflict);
        result.Error.Message.Should().Be("No available seats for this event");

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _eventsRepository.DidNotReceive().Save(Arg.Any<Event>(), Arg.Any<CancellationToken>());
        await _queueMessageRepository.DidNotReceive().Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Get_Should_ReturnNotFound_When_BookingDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        _bookingRepository.Get(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Booking?) null);

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

    [Test]
    public async Task Reject_And_ReleaseSeats_Should_AllowNewBooking_ForSameSeat()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var @event = CreateTestEvent(eventId, totalSeats: 5, availableSeats: 0);
        var booking = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);
        var newBooking = CreateValidBooking(eventId: eventId, status: BookingStatus.Pending);

        booking.Reject(DateTime.UtcNow);
        @event.ReleaseSeats();

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(Result<Booking, Error>.FromSuccess(newBooking));

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _bookingService.Create(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        @event.AvailableSeats.Should().Be(0);

        await _bookingRepository.Received(1).Save(Arg.Is<Booking>(b =>
            b.Id == newBooking.Id), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Concurrency Tests

    [Test]
    public async Task Create_Should_ProtectAgainstOverbooking()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var @event = CreateTestEvent(eventId, totalSeats: 5);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(_ => Result<Booking, Error>.FromSuccess(
                CreateValidBooking(eventId: eventId)));

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var tasks = Enumerable.Range(0, 20).Select(_ => _bookingService.Create(eventId));

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        var successCount = results.Count(r => r.IsSuccess);
        var conflictCount = results.Count(r =>
            r.IsFailure && r.Error.StatusCode == ErrorCode.Conflict);

        successCount.Should().Be(5);
        conflictCount.Should().Be(15);
        @event.AvailableSeats.Should().Be(0);
    }

    [Test]
    public async Task Create_Should_GenerateUniqueIds_UnderConcurrency()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        var @event = CreateTestEvent(eventId, totalSeats: 10);

        _eventsRepository.Get(eventId, Arg.Any<CancellationToken>())
            .Returns(@event);

        _bookingFactory.Create(eventId, Arg.Any<CancellationToken>())
            .Returns(_ => Result<Booking, Error>.FromSuccess(
                CreateValidBooking(eventId: eventId)));

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _eventsRepository.Save(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _queueMessageRepository.Create(Arg.Any<QueueMessage<BookingCreatedMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var tasks = Enumerable.Range(0, 10).Select(_ => _bookingService.Create(eventId));

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.All(r => r.IsSuccess).Should().BeTrue();
        results.Select(r => r.Value.Id).Distinct().Should().HaveCount(10);
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

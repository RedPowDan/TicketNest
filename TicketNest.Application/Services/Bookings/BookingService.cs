using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Bookings;

internal sealed class BookingService(
    IBookingFactory bookingFactory,
    IBookingRepository bookingRepository,
    IQueueMessageRepository queueMessageRepository,
    IEventsRepository eventsRepository,
    IUserRepository userRepository) : IBookingService
{
    private const int MaxBookingsByUser = 10; 
    
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    public async Task<Result<Booking, Error>> Create(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var bookingCreateResult = await bookingFactory.Create(eventId, userId, ct);
        if (bookingCreateResult.IsFailure)
        {
            return bookingCreateResult;
        }

        var booking = bookingCreateResult.Value;

        await SemaphoreSlim.WaitAsync(ct);
        try
        {
            var @event = await eventsRepository.Get(eventId, ct);
            if (@event is null)
            {
                return new Error(message: "Событие не найдено", statusCode: ErrorCode.NotFound);
            }

            if (@event.TryReserveSeats(DateTime.UtcNow) is { IsFailure: true } result)
            {
                return result.Error;
            }

            var bookingsByUser = await bookingRepository.GetBookingsByUserId(userId, ct);
            if (bookingsByUser.Length > MaxBookingsByUser)
            {
                return new Error(ErrorCode.Conflict, "Невозможно забронировать более 10 мест для события");
            }

            await bookingRepository.Save(booking, ct);
            await eventsRepository.Save(@event, ct);
            await queueMessageRepository.Create(CreateMessage(booking.Id), ct);
        }
        finally
        {
            SemaphoreSlim.Release();
        }

        return booking;
    }

    public async Task<Result<Booking, Error>> Get(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingRepository.Get(id, ct);
        if (booking == null)
        {
            return new Error(ErrorCode.NotFound, "Бронирование не найдено");
        }

        return booking;
    }

    public async Task<Result<Booking, Error>> Cancel(Guid bookingId, Guid userId, CancellationToken ct = default)
    {
        var initiator = await userRepository.Get(userId, ct);
        if (initiator is null)
        {
            return new Error(ErrorCode.Unauthorized, "Текущий пользователь не найден");
        }

        var booking = await bookingRepository.Get(bookingId, ct);
        if (booking is null)
        {
            return new Error(ErrorCode.NotFound, "Бронь не найдена");
        }

        var canCancel = booking.CanCancel(initiator);
        if (canCancel.IsFailure)
        {
            return canCancel.Error;
        }

        booking.Cancel(DateTime.UtcNow);

        await bookingRepository.Save(booking, ct);
        
        var message = QueueMessage<BookingCanceledMessage>.Create(queueName: QueueNames.BookingQueue, new BookingCanceledMessage(bookingId: bookingId));
        await queueMessageRepository.Create(message, ct);

        return booking;
    }

    private static QueueMessage<BookingCreatedMessage> CreateMessage(Guid bookingId)
    {
        return QueueMessage<BookingCreatedMessage>.Create(queueName: QueueNames.BookingQueue, new BookingCreatedMessage(bookingId: bookingId));
    }
}
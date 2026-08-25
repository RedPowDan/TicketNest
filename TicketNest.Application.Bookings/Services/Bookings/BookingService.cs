using TicketNest.Domain.Bookings.Constants;
using TicketNest.Domain.Bookings.Models;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Models.Users;
using TicketNest.Domain.Bookings.Repositories;
using TicketNest.Domain.Bookings.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Bookings.Services.Bookings;

internal sealed class BookingService(
    IBookingFactory bookingFactory,
    IBookingRepository bookingRepository/*,
    IQueueMessageRepository queueMessageRepository*/) : IBookingService
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
            var bookingsByUser = await bookingRepository.GetBookingsByUserId(userId, ct);
            var activeBookingsByUser = bookingsByUser.Where(x => x.IsActive());
            if (activeBookingsByUser.Count() >= MaxBookingsByUser)
            {
                return new Error(ErrorCode.Conflict, $"Невозможно забронировать более {MaxBookingsByUser} активных броней");
            }

            await bookingRepository.Save(booking, ct);
            //await queueMessageRepository.Create(CreateMessage(booking.Id), ct);
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

    public async Task<Result<Booking, Error>> Cancel(Guid bookingId, Guid userId, UserRole userRole, CancellationToken ct = default)
    {
        var booking = await bookingRepository.Get(bookingId, ct);
        if (booking is null)
        {
            return new Error(ErrorCode.NotFound, "Бронь не найдена");
        }

        var canCancel = booking.CanCancel(userId: userId, userRole: userRole);
        if (canCancel.IsFailure)
        {
            return canCancel.Error;
        }

        booking.Cancel(DateTime.UtcNow);

        await bookingRepository.Save(booking, ct);
        
        //var message = QueueMessage<BookingCanceledMessage>.Create(queueName: QueueNames.BookingCancelledQueue, new BookingCanceledMessage(bookingId: bookingId));
        //await queueMessageRepository.Create(message, ct);

        return booking;
    }

    // private static QueueMessage<BookingCreatedMessage> CreateMessage(Guid bookingId)
    // {
    //     return QueueMessage<BookingCreatedMessage>.Create(queueName: QueueNames.BookingCreatedQueue, new BookingCreatedMessage(bookingId: bookingId));
    // }
}
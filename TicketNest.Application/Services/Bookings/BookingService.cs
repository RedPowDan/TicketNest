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
    IQueueMessageRepository queueMessageRepository) : IBookingService
{
    public async Task<Result<Booking, Error>> Create(Guid eventId, CancellationToken ct = default)
    {
        var bookingCreateResult = await bookingFactory.Create(eventId, ct);
        if (bookingCreateResult.IsFailure)
        {
            return bookingCreateResult;
        }

        var booking = bookingCreateResult.Value;
        await bookingRepository.Save(booking, ct);
        await queueMessageRepository.Create(CreateMessage(booking.Id), ct);

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

    private static QueueMessage<BookingCreatedMessage> CreateMessage(Guid bookingId)
    {
        return QueueMessage<BookingCreatedMessage>.Create(queueName: QueueNames.BookingQueue, new BookingCreatedMessage(bookingId: bookingId));
    }
}
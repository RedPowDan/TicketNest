using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;
using TicketNest.Domain.Bookings.Services;

namespace TicketNest.Domain.Bookings.Outbox.Handlers;

internal sealed class BookingCanceledEventHandler(IBookingProducer bookingProducer, ILogger<BookingCanceledEventHandler> logger)
    : IEventHandler<BookingCanceled>
{
    public async Task HandleAsync(BookingCanceled e, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Бронь отменена: {BookingId} (мероприятие {EventId})",
            e.BookingId,
            e.EventId);

        await bookingProducer.BookingCanceled(e.BookingId, e.EventId, ct);
    }
}

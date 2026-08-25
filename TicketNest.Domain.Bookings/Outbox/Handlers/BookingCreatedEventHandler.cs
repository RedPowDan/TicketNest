using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;
using TicketNest.Domain.Bookings.Services;

namespace TicketNest.Domain.Bookings.Outbox.Handlers;

internal sealed class BookingCreatedEventHandler(IBookingProducer bookingProducer, ILogger<BookingCreatedEventHandler> logger)
    : IEventHandler<BookingCreated>
{
    public async Task HandleAsync(BookingCreated e, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Бронь создана: {BookingId}, мероприятие {EventId}",
            e.BookingId,
            e.EventId);

        await bookingProducer.BookingCreated(e.BookingId, e.EventId, ct);
    }
}
using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.Domain.Bookings.Outbox.Handlers;

/// <summary>
/// Обработчик создания брони (слой Application). Здесь, например, публикация события в шину/Kafka.
/// </summary>
internal sealed class BookingCreatedEventHandler(ILogger<BookingCreatedEventHandler> logger)
    : IEventHandler<BookingCreated>
{
    public Task HandleAsync(BookingCreated e, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Бронь создана: {BookingId}, мероприятие {EventId}",
            e.BookingId,
            e.EventId);

        return Task.CompletedTask;
    }
}

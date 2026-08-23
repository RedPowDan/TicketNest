using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.Domain.Bookings.Outbox.Handlers;

/// <summary>
/// Первый обработчик отмены брони: публикация события отмены.
/// </summary>
internal sealed class BookingCanceledEventHandler(ILogger<BookingCanceledEventHandler> logger)
    : IEventHandler<BookingCanceled>
{
    public Task HandleAsync(BookingCanceled e, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Бронь отменена: {BookingId} (мероприятие {EventId})",
            e.BookingId,
            e.EventId);

        return Task.CompletedTask;
    }
}

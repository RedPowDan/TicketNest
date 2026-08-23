using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.Domain.Bookings.Outbox.Handlers;

/// <summary>
/// Обработчик отклонения брони (слой Application).
/// </summary>
internal sealed class BookingRejectedEventHandler(ILogger<BookingRejectedEventHandler> logger)
    : IEventHandler<BookingRejected>
{
    public Task HandleAsync(BookingRejected e, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Бронь отклонена: {BookingId} (мероприятие {EventId})",
            e.BookingId,
            e.EventId);

        return Task.CompletedTask;
    }
}

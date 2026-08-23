using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.Domain.Bookings.Outbox.Handlers;

/// <summary>
/// Второй обработчик отмены брони: например, отправка уведомления.
/// Демонстрирует, что на один тип события можно повесить несколько обработчиков.
/// </summary>
internal sealed class BookingCanceledNotificationHandler(ILogger<BookingCanceledNotificationHandler> logger)
    : IEventHandler<BookingCanceled>
{
    public Task HandleAsync(BookingCanceled e, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Уведомление об отмене брони отправлено по брони {BookingId}",
            e.BookingId);

        return Task.CompletedTask;
    }
}

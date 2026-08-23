using TicketNest.DataAccess.Bookings.Outbox.PersistenceEvents;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.DataAccess.Bookings.Outbox.Mappers;

/// <summary>
/// Маппер между доменными outbox-событиями и их персистанс-моделями.
/// Позволяет отказаться от прямой сериализации доменных событий в OutboxMessage.
/// </summary>
internal static class OutboxEventMapper
{
    public static BookingPersistenceEvent ToPersistence(IBookingEvent domainEvent)
    {
        return domainEvent switch
        {
            BookingCreated e => new BookingCreatedPersistenceEvent
            {
                BookingId = e.BookingId,
                EventId = e.EventId,
            },
            BookingCanceled e => new BookingCanceledPersistenceEvent
            {
                BookingId = e.BookingId,
                EventId = e.EventId,
            },
            BookingRejected e => new BookingRejectedPersistenceEvent
            {
                BookingId = e.BookingId,
                EventId = e.EventId,
            },
            _ => throw new InvalidOperationException(
                $"Неизвестный тип доменного события: {domainEvent.GetType().FullName}"),
        };
    }

    public static IBookingEvent ToDomain(BookingPersistenceEvent persistenceEvent)
    {
        return persistenceEvent switch
        {
            BookingCreatedPersistenceEvent e => new BookingCreated(e.BookingId, e.EventId),
            BookingCanceledPersistenceEvent e => new BookingCanceled(e.BookingId, e.EventId),
            BookingRejectedPersistenceEvent e => new BookingRejected(e.BookingId, e.EventId),
            _ => throw new InvalidOperationException(
                $"Неизвестный тип персистанс-события: {persistenceEvent.GetType().FullName}"),
        };
    }
}

using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Bookings;

namespace TicketNest.DataAccess.Events.Mappers;

internal static class BookingMapper
{
    public static Booking ToDomain(PersistenceBooking source)
    {
        return Booking.LoadFromStorage(
            id: source.Id,
            eventId: source.EventId,
            status: source.Status,
            createdAt: source.CreatedAt,
            processedAt: source.ProcessedAt);
    }

    public static PersistenceBooking ToPersistence(Booking source)
    {
        return new PersistenceBooking
        {
            Id = source.Id,
            EventId = source.EventId,
            Status = source.Status,
            CreatedAt = source.CreatedAt,
            ProcessedAt = source.ProcessedAt,
        };
    }
}
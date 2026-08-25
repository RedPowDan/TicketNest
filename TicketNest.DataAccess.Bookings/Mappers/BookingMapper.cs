using TicketNest.DataAccess.Bookings.Models;
using TicketNest.Domain.Bookings.Models.Bookings;

namespace TicketNest.DataAccess.Bookings.Mappers;

internal static class BookingMapper
{
    public static Booking ToDomain(PersistenceBooking source)
    {
        return Booking.LoadFromStorage(
            id: source.Id,
            eventId: source.EventId,
            userId: source.UserId,
            status: source.Status,
            createdAt: source.CreatedAt,
            processedAt: source.ProcessedAt);
    }

    public static void Map(Booking source, PersistenceBooking target)
    {
        target.Id = source.Id;
        target.EventId = source.EventId;
        target.UserId = source.UserId;
        target.Status = source.Status;
        target.CreatedAt = source.CreatedAt;
        target.ProcessedAt = source.ProcessedAt;
    }

    public static PersistenceBooking ToPersistence(Booking source)
    {
        var persistance = new PersistenceBooking();

        Map(source, persistance);

        return persistance;
    }
}
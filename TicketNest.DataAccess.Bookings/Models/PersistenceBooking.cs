using TicketNest.Domain.Bookings.Models.Bookings;

namespace TicketNest.DataAccess.Bookings.Models;

internal sealed class PersistenceBooking
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Guid UserId { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
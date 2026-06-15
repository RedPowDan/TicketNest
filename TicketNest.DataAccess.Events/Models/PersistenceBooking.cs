using TicketNest.Domain.Models.Bookings;

namespace TicketNest.DataAccess.Events.Models;

internal sealed class PersistenceBooking
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public PersistenceEvent Event { get; set; } = null!;
}
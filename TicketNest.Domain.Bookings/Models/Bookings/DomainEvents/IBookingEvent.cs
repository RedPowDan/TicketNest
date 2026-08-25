namespace TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

public interface IBookingEvent
{
    public Guid BookingId { get; }
}
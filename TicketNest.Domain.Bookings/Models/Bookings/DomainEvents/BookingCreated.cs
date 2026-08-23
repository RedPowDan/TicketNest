namespace TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

public class BookingCreated : IBookingEvent
{
    public Guid BookingId { get; }

    public Guid EventId { get; }

    public BookingCreated(Guid bookingId, Guid eventId)
    {
        Ensure.NotDefault(bookingId, nameof(bookingId));
        Ensure.NotDefault(eventId, nameof(eventId));
        
        BookingId = bookingId;
        EventId = eventId;
    }
}
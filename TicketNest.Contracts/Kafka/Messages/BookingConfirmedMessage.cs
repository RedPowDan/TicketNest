namespace TicketNest.Contracts.Kafka.Messages;

public class BookingConfirmedMessage
{
    public Guid BookingId { get; }

    public Guid EventId { get; }

    public BookingConfirmedMessage(Guid bookingId, Guid eventId)
    {
        BookingId = bookingId;
        EventId = eventId;
    }
}
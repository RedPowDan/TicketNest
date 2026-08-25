namespace TicketNest.Contracts.Kafka.Messages;

public class BookingApprovedMessage
{
    public Guid BookingId { get; }

    public Guid EventId { get; }

    public BookingApprovedMessage(Guid bookingId, Guid eventId)
    {
        BookingId = bookingId;
        EventId = eventId;
    }
}
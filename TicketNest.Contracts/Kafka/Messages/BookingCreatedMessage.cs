namespace TicketNest.Contracts.Kafka.Messages;

public class BookingCreatedMessage
{
    public Guid BookingId { get; }

    public Guid EventId { get; }

    public BookingCreatedMessage(Guid bookingId, Guid eventId)
    {
        BookingId = bookingId;
        EventId = eventId;
    }
}
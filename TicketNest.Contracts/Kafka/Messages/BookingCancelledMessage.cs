namespace TicketNest.Contracts.Kafka.Messages;

public class BookingCancelledMessage
{
    public Guid BookingId { get; }

    public Guid EventId { get; }

    public BookingCancelledMessage(Guid bookingId, Guid eventId)
    {
        BookingId = bookingId;
        EventId = eventId;
    }
}
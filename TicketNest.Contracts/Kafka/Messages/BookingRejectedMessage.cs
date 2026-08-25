namespace TicketNest.Contracts.Kafka.Messages;

public class BookingRejectedMessage
{
    public Guid BookingId { get; }

    public Guid EventId { get; }

    public string Reason { get; }

    public BookingRejectedMessage(Guid bookingId, Guid eventId, string reason)
    {
        BookingId = bookingId;
        EventId = eventId;
        Reason = reason;
    }
}
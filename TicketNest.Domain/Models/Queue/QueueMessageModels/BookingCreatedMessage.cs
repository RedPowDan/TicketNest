namespace TicketNest.Domain.Models.Queue.QueueMessageModels;

public class BookingCreatedMessage
{
    public Guid BookingId { get; }

    public BookingCreatedMessage(Guid bookingId)
    {
        Ensure.NotDefault(bookingId, nameof(bookingId));

        BookingId = bookingId;
    }
}
namespace TicketNest.Domain.Models.Queue.QueueMessageModels;

public class BookingCanceledMessage
{
    public Guid BookingId { get; }

    public BookingCanceledMessage(Guid bookingId)
    {
        Ensure.NotDefault(bookingId, nameof(bookingId));

        BookingId = bookingId;
    }
}

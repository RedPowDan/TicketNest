namespace TicketNest.Api.Models.V1.Bookings;

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
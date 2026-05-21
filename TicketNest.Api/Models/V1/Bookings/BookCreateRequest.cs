namespace TicketNest.Api.Models.V1.Bookings;

public sealed class BookCreateRequest
{
    /// <summary>
    /// Идентификатор события, на которое создается бронь
    /// </summary>
    public Guid EventId { get; set; }
}
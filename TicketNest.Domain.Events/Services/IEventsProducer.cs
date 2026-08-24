namespace TicketNest.Domain.Events.Services;

public interface IEventsProducer
{
    Task BookingRejected(Guid bookingId, Guid eventId, string reason, CancellationToken ct);
    
    Task BookingConfirmed(Guid bookingId, Guid eventId, CancellationToken ct);
}
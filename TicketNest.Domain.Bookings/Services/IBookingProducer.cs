namespace TicketNest.Domain.Bookings.Services;

public interface IBookingProducer
{
    public Task BookingCreated(Guid bookingId, Guid eventId, CancellationToken ct);
    public Task BookingCanceled(Guid bookingId, Guid eventId, CancellationToken ct);
}
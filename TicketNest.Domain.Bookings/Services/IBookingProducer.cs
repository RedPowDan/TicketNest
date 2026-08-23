using TicketNest.Domain.Bookings.Models.Bookings;

namespace TicketNest.Domain.Bookings.Services;

public interface IBookingProducer
{
    public Task BookingCreated(Booking booking, CancellationToken ct);
}
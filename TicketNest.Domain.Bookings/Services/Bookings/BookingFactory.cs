using TicketNest.Domain.Bookings.Models;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Bookings.Services.Bookings;

public class BookingFactory : IBookingFactory
{
    public Task<Result<Booking, Error>> Create(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult<Result<Booking, Error>>(Booking.Create(eventId, userId, createdAt: DateTime.UtcNow));
    }
}
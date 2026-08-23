using TicketNest.Domain.Bookings.Models;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Bookings.Services.Bookings;

public interface IBookingFactory
{
    public Task<Result<Booking, Error>> Create(Guid eventId, Guid userId, CancellationToken ct = default);
}
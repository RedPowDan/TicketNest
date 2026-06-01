using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Bookings;

public interface IBookingFactory
{
    public Task<Result<Booking, Error>> Create(Guid eventId, CancellationToken ct = default);
}
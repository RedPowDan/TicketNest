using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Bookings;

public interface IBookingService
{
    Task<Result<Booking, Error>> Create(Guid eventId, CancellationToken ct = default);

    Task<Result<Booking, Error>> Get(Guid id, CancellationToken ct = default);
}
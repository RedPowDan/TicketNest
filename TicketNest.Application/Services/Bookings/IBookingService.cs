using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Bookings;

public interface IBookingService
{
    Task<Result<Booking, Error>> Create(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<Result<Booking, Error>> Get(Guid id, CancellationToken ct = default);

    Task<Result<Booking, Error>> Cancel(Guid bookingId, Guid userId, CancellationToken ct = default);
}
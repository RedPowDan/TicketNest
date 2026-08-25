using TicketNest.Domain.Bookings.Models;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Models.Users;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Bookings.Services.Bookings;

public interface IBookingService
{
    Task<Result<Booking, Error>> Create(Guid eventId, Guid userId, CancellationToken ct = default);

    Task<Result<Booking, Error>> Get(Guid id, CancellationToken ct = default);

    Task<Result<Booking, Error>> Cancel(Guid bookingId, Guid userId, UserRole userRole, CancellationToken ct = default);
}
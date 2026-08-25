using TicketNest.Domain.Bookings.Models;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Bookings.Services.Bookings;

public interface IBookingConfirmationService
{
    Task<UnitResult<Error>> Confirm(Guid bookingId, CancellationToken ct);
}
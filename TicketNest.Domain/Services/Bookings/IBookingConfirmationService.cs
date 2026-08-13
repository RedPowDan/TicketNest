using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Bookings;

public interface IBookingConfirmationService
{
    Task<UnitResult<Error>> Confirm(Guid bookingId, CancellationToken ct);
}
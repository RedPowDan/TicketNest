using TicketNest.Domain.Models;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Events;

public interface IEventReleaseSeatsService
{
    Task<UnitResult<Error>> ReleaseSeats(Guid bookingId, int count = 1, CancellationToken ct = default);
}
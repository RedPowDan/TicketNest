using TicketNest.Domain.Events.Models;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Events.Services.Events;

public interface IEventReleaseSeatsService
{
    Task<UnitResult<Error>> ReleaseSeats(Guid eventId, int count = 1, CancellationToken ct = default);
}
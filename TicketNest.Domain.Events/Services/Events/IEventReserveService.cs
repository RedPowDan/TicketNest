using TicketNest.Domain.Events.Models;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Events.Services.Events;

public interface IEventReserveService
{
    Task<UnitResult<Error>> Reserve(Guid eventId, DateTime reserveDateTime, CancellationToken ct);
}
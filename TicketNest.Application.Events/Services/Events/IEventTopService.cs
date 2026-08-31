using TicketNest.Domain.Events.Models.Events;

namespace TicketNest.Application.Events.Services.Events;

public interface IEventTopService
{
    Task<IReadOnlyList<Event>> GetTop10(CancellationToken ct = default);
}

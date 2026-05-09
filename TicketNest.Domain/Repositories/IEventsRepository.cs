using TicketNest.Domain.Filters;
using TicketNest.Domain.Models.Events;

namespace TicketNest.Domain.Repositories;

public interface IEventsRepository
{
    Task Save(Event @event, CancellationToken ct = default);

    ValueTask<Event?> Get(Guid id, CancellationToken ct = default);

    Task<IReadOnlyCollection<Event>> GetAll(EventsFilter filter, CancellationToken ct = default);

    Task<bool> Remove(Guid id, CancellationToken ct = default);
}
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.ValueObjects;

namespace TicketNest.Domain.Repositories;

public interface IEventsRepository
{
    Task Save(Event @event, CancellationToken ct = default);

    ValueTask<Event?> Get(EventId id, CancellationToken ct = default);

    Task<IReadOnlyCollection<Event>> GetAll(CancellationToken ct = default);

    Task<bool> Remove(EventId id, CancellationToken ct = default);
}
using System.Collections.Concurrent;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Repositories;

namespace TicketNest.DataAccess.Events.Implementations;

internal sealed class EventRepository : IEventsRepository
{
    private static readonly ConcurrentDictionary<Guid, PersistenceEvent> Events = new();

    public Task Save(Event @event, CancellationToken ct = default)
    {
        Ensure.NotNull(@event, nameof(@event));

        var persistenceEvent = EventMapper.ToPersistence(@event);

        Events.AddOrUpdate(persistenceEvent.Id, persistenceEvent, (_, _) => persistenceEvent);

        return Task.CompletedTask;
    }

    public ValueTask<Event?> Get(Guid id, CancellationToken ct = default)
    {
        Events.TryGetValue(id, out var persistenceEvent);

        return ValueTask.FromResult(persistenceEvent == null ? null : EventMapper.ToDomain(persistenceEvent));
    }

    public Task<IReadOnlyCollection<Event>> GetAll(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyCollection<Event>>(Events.Values.Select(EventMapper.ToDomain).ToArray());
    }

    public Task<bool> Remove(Guid id, CancellationToken ct = default)
    {
        Events.TryRemove(id, out var persistenceEvent);
        return Task.FromResult(persistenceEvent != null);
    }
}
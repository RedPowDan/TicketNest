using System.Collections.Concurrent;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.ValueObjects;

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

    public ValueTask<Event?> Get(EventId id, CancellationToken ct = default)
    {
        Ensure.NotNull(id, nameof(id));

        Events.TryGetValue(id.Value, out var persistenceEvent);

        return ValueTask.FromResult(persistenceEvent == null ? null : EventMapper.ToDomain(persistenceEvent));
    }

    public Task Remove(EventId id, CancellationToken ct = default)
    {
        Ensure.NotNull(id, nameof(id));

        Events.TryRemove(id.Value, out _);
        return Task.CompletedTask;
    }
}
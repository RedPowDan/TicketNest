using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Events;

namespace TicketNest.DataAccess.Events.Mappers;

internal static class EventMapper
{
    public static Event ToDomain(PersistenceEvent source)
    {
        Ensure.NotNull(source, nameof(source));

        return Event.LoadFromStorage(id: source.Id,
            title: source.Title,
            description: source.Description,
            startAt: source.StartAt,
            endAt: source.EndAt);
    }

    public static PersistenceEvent ToPersistence(Event source)
    {
        Ensure.NotNull(source, nameof(source));

        return new PersistenceEvent
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            EndAt = source.EndAt,
            StartAt = source.StartAt,
        };
    }
}
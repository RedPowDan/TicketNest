using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.ValueObjects;

namespace TicketNest.DataAccess.Events.Mappers;

internal static class EventMapper
{
    public static Event ToDomain(PersistenceEvent source)
    {
        return Event.LoadFromStorage(id: EventId.From(source.Id),
            title: EventTitle.From(source.Title),
            description: source.Description == null ? null : EventDescription.From(source.Description),
            startAt: source.StartAt,
            endAt: source.EndAt);
    }

    public static PersistenceEvent ToPersistence(Event source)
    {
        return new PersistenceEvent
        {
            Id = source.Id.Value,
            Title = source.Title.Value,
            Description = source.Description?.Value,
            EndAt = source.EndAt,
            StartAt = source.StartAt,
        };
    }
}
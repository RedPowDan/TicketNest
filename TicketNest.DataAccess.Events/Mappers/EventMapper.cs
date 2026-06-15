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
            endAt: source.EndAt,
            totalSeats: source.TotalSeats,
            availableSeats: source.AvailableSeats);
    }

    public static void Map(Event source, PersistenceEvent target)
    {
        Ensure.NotNull(source, nameof(source));
        Ensure.NotNull(target, nameof(target));

        target.Id = source.Id;
        target.Title = source.Title;
        target.Description = source.Description;
        target.EndAt = source.EndAt;
        target.StartAt = source.StartAt;
        target.TotalSeats = source.TotalSeats;
        target.AvailableSeats = source.AvailableSeats;
    }

    public static PersistenceEvent ToPersistence(Event source)
    {
        Ensure.NotNull(source, nameof(source));

        var persistenceEvent = new PersistenceEvent();

        Map(source, persistenceEvent);

        return persistenceEvent;
    }
}
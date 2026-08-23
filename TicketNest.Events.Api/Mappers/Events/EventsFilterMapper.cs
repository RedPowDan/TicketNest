using TicketNest.Domain.Events.Filters;
using TicketNest.Events.Api.Models.V1.Events;

namespace TicketNest.Events.Api.Mappers.Events;

internal static class EventsFilterMapper
{
    public static EventsFilter Map(EventsFilterModel source)
    {
        return new EventsFilter(title: source.Title, from: source.From, to: source.To);
    }
}
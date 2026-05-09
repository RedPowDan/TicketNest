using TicketNest.Api.Models.V1.Events;
using TicketNest.Domain.Filters;

namespace TicketNest.Api.Mappers.Events;

internal static class EventsFilterMapper
{
    public static EventsFilter Map(EventsFilterModel source)
    {
        return new EventsFilter(title: source.Title, from: source.From, to: source.To);
    }
}
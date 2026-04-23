using TicketNest.Api.Models.V1.Events;
using TicketNest.Domain.Models.Events;

namespace TicketNest.Api.Mappers.Events;

public static class EventResponseMapper
{
    public static EventResponse Map(Event source)
    {
        return new EventResponse
        {
            Id = source.Id.Value,
            Title = source.Title.Value,
            Description = source.Description?.Value,
            EndAt = source.EndAt,
            StartAt = source.StartAt,
        };
    }
}
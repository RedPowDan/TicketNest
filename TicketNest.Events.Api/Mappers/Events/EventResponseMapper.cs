using TicketNest.Domain.Events.Models.Events;
using TicketNest.Events.Api.Models.V1.Events;

namespace TicketNest.Events.Api.Mappers.Events;

public static class EventResponseMapper
{
    public static EventResponse Map(Event source)
    {
        return new EventResponse
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            EndAt = source.EndAt,
            StartAt = source.StartAt,
            TotalSeats = source.TotalSeats,
            AvailableSeats = source.AvailableSeats,
        };
    }
}
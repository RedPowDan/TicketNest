namespace TicketNest.Api.Models.V1.Events;

public sealed class EventsFilterModel
{
    public string? Title { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
}
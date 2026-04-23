using System.ComponentModel.DataAnnotations;

namespace TicketNest.Api.Models.V1.Events;

public class EventResponse
{
    [Required] public Guid Id { get; init; }

    [Required] public string Title { get; init; } = null!;

    public string? Description { get; init; }

    [Required] public DateTime StartAt { get; init; }

    [Required] public DateTime EndAt { get; init; }
}
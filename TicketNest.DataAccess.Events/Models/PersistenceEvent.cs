namespace TicketNest.DataAccess.Events.Models;

internal sealed class PersistenceEvent
{
    public Guid Id { get; init; }

    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public DateTime StartAt { get; init; }

    public DateTime EndAt { get; init; }

    public int TotalSeats { get; init; }

    public int AvailableSeats { get; init; }
}
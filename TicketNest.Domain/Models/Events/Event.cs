using TicketNest.Domain.ValueObjects;

namespace TicketNest.Domain.Models.Events;

public class Event
{
    public EventId Id { get; }

    public EventTitle Title { get; }

    public EventDescription? Description { get; }

    public DateTime StartAt { get; }

    public DateTime EndAt { get; }

    private Event(EventId id, EventTitle title, EventDescription? description, DateTime startAt, DateTime endAt)
    {
        Ensure.NotNull(id, nameof(id));
        Ensure.NotNull(title, nameof(title));
        Ensure.NotDefault(startAt, nameof(startAt));
        Ensure.NotDefault(endAt, nameof(endAt));

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public static Event LoadFromStorage(EventId id, EventTitle title, EventDescription? description, DateTime startAt, DateTime endAt)
    {
        return new Event(id, title, description, startAt, endAt);
    }
}
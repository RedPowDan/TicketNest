using TicketNest.Domain.ValueObjects;
using TicketNest.Shared.Objects;

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

    public static Result<Event, string> Create(EventTitle title, EventDescription? description, DateTime startAt, DateTime endAt)
    {
        if (CanCreate(title, description, startAt, endAt) is { IsSuccess: true } result)
        {
            return result.Error;
        }

        return new Event(EventId.New(), title, description, startAt, endAt);
    }

    private static UnitResult<string> CanCreate(EventTitle title, EventDescription? description, DateTime startAt, DateTime endAt)
    {
        if (title == null)
        {
            return "Название события не должно быть пустое";
        }

        if (startAt == default)
        {
            return "Начало события не может быть значением по умолчанию";
        }

        if (startAt > endAt)
        {
            return "Начало события не может быть больше чем его окончание";
        }

        return UnitResult<string>.FromSuccess();
    }
}
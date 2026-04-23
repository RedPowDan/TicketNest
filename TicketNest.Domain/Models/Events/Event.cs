using TicketNest.Domain.ValueObjects;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Models.Events;

public class Event
{
    public EventId Id { get; }

    public EventTitle Title { get; private set; }

    public EventDescription? Description { get; private set; }

    public DateTime StartAt { get; private set; }

    public DateTime EndAt { get; private set; }

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
        if (CanCreate(title, startAt, endAt) is { IsFailure: true } result)
        {
            return result.Error;
        }

        return new Event(EventId.New(), title, description, startAt, endAt);
    }

    private static UnitResult<string> CanCreate(EventTitle title, DateTime startAt, DateTime endAt)
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

    public UnitResult<string> ChangeTitle(EventTitle title)
    {
        Ensure.NotNull(title, nameof(title));

        Title = title;

        return UnitResult<string>.FromSuccess();
    }

    public UnitResult<string> ChangeDescription(EventDescription? description)
    {
        Description = description;

        return UnitResult<string>.FromSuccess();
    }

    public UnitResult<string> ChangeStartAtAndEndAt(DateTime startAt, DateTime endAt)
    {
        if (startAt == default)
        {
            return "Начало события не может быть значением по умолчанию";
        }

        if (startAt > endAt)
        {
            return "Начало события не может быть больше чем его окончание";
        }

        StartAt = startAt;
        EndAt = endAt;

        return UnitResult<string>.FromSuccess();
    }
}
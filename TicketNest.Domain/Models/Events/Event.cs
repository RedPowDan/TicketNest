using TicketNest.Shared;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Models.Events;

public class Event
{
    public Guid Id { get; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public DateTime StartAt { get; private set; }

    public DateTime EndAt { get; private set; }

    private Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
        Ensure.NotNullOrEmpty(title, nameof(title));
        if (description != null)
        {
            Ensure.NotNullOrEmpty(description, nameof(description));
        }
        Ensure.NotNull(title, nameof(title));
        Ensure.NotDefault(startAt, nameof(startAt));
        Ensure.NotDefault(endAt, nameof(endAt));

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public static Event LoadFromStorage(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
        return new Event(id, title, description, startAt, endAt);
    }

    public static Result<Event, string> Create(string title, string? description, DateTime startAt, DateTime endAt)
    {
        if (CanCreate(title, description, startAt, endAt) is { IsFailure: true } result)
        {
            return result.Error;
        }

        return new Event(SequentialGuidFactory.Create(DateTime.UtcNow), title, description, startAt, endAt);
    }

    private static UnitResult<string> CanCreate(string title, string? description, DateTime startAt, DateTime endAt)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Название события не должно быть пустое";
        }

        if (description != null && string.IsNullOrEmpty(description))
        {
            return "Описание не может быть пустой строкой";
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

    public UnitResult<string> ChangeTitle(string title)
    {
        Ensure.NotNullOrEmpty(title, nameof(title));

        Title = title;

        return UnitResult<string>.FromSuccess();
    }

    public UnitResult<string> ChangeDescription(string? description)
    {
        if (description != null)
        {
            Ensure.NotNullOrEmpty(description, nameof(description));
        }

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
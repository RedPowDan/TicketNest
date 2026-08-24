using TicketNest.Domain.Events.Constants;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Events.Models.Events;

public class Event
{
    public Guid Id { get; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public DateTime StartAt { get; private set; }

    public DateTime EndAt { get; private set; }

    public int TotalSeats { get; private set; }

    public int AvailableSeats { get; private set; }

    private Event(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats,
        int availableSeats)
    {
        Ensure.NotNullOrEmpty(title, nameof(title));
        if (description != null)
        {
            Ensure.NotNullOrEmpty(description, nameof(description));
        }

        Ensure.NotNull(title, nameof(title));
        Ensure.NotDefault(startAt, nameof(startAt));
        Ensure.NotDefault(endAt, nameof(endAt));
        Ensure.NonNegative(totalSeats, nameof(totalSeats));

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = availableSeats;
    }

    public static Event LoadFromStorage(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats,
        int availableSeats)
    {
        if (totalSeats < 0)
        {
            totalSeats = 0;
        }

        return new Event(id, title, description, startAt, endAt, totalSeats, availableSeats);
    }

    public static Result<Event, string> Create(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        if (CanCreate(title, description, startAt, endAt, totalSeats) is { IsFailure: true } result)
        {
            return result.Error;
        }

        return new Event(Guid.CreateVersion7(), title, description, startAt, endAt, totalSeats, availableSeats: totalSeats);
    }

    private static UnitResult<string> CanCreate(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
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

        if (totalSeats <= 0)
        {
            return "Количество мест должно быть больше нуля";
        }

        return UnitResult<string>.FromSuccess();
    }

    public UnitResult<string> ChangeTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return "Название не может быть пустым";
        }

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

    internal UnitResult<Error> TryReserveSeats(DateTime now, int count = 1)
    {
        Ensure.NonNegative(count, nameof(count));

        if (count > AvailableSeats)
        {
            return new Error(message: "Нет доступных мест для этого события", statusCode: ErrorCode.Conflict);
        }

        if (EventIsStarted(now))
        {
            return new Error(message: "Событие уже началось", statusCode: ErrorCode.BadRequest);
        }

        AvailableSeats -= count;

        return UnitResult<Error>.FromSuccess();
    }
    
    public bool ReleaseSeats(int count = 1)
    {
        Ensure.NonNegative(count, nameof(count));

        if (count + AvailableSeats > TotalSeats)
        {
            return false;
        }

        AvailableSeats += count;

        return true;
    }

    private bool EventIsStarted(DateTime now) => StartAt <= now;
}
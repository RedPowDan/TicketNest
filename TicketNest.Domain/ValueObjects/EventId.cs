using TicketNest.Shared;

namespace TicketNest.Domain.ValueObjects;

public record EventId
{
    public Guid Value { get; }

    private EventId(Guid value)
    {
        Value = value;
    }

    public static EventId New() => new EventId(SequentialGuidFactory.Create(DateTime.UtcNow));

    public static EventId From(Guid value) => new EventId(value);
}
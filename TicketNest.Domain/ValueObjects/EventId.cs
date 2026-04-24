using TicketNest.Shared;

namespace TicketNest.Domain.ValueObjects;

public record EventId : ValueObject
{
    public Guid Value { get; }

    private EventId(Guid value)
    {
        Value = value;
    }

    public static EventId New() => new EventId(SequentialGuidFactory.Create(DateTime.UtcNow));

    public static EventId From(Guid value) => new EventId(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
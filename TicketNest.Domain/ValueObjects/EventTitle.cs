namespace TicketNest.Domain.ValueObjects;

public record EventTitle : ValueObject
{
    public string Value { get; }

    private EventTitle(string value)
    {
        Ensure.NotNullOrEmpty(value, nameof(value));

        Value = value;
    }

    public static EventTitle From(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
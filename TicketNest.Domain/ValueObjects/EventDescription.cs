namespace TicketNest.Domain.ValueObjects;

public record EventDescription  : ValueObject
{
    public string Value { get; }

    private EventDescription(string value)
    {
        Ensure.NotNullOrEmpty(value, nameof(value));

        Value = value;
    }

    public static EventDescription From(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
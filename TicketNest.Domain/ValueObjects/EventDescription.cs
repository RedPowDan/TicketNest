namespace TicketNest.Domain.ValueObjects;

public record EventDescription
{
    public string Value { get; }

    private EventDescription(string value)
    {
        Ensure.NotNullOrEmpty(value, nameof(value));

        Value = value;
    }

    public static EventDescription From(string value) => new(value);
}
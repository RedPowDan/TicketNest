namespace TicketNest.Domain.ValueObjects;

public class EventTitle
{
    public string Value { get; }

    private EventTitle(string value)
    {
        Ensure.NotNullOrEmpty(value, nameof(value));

        Value = value;
    }

    public static EventTitle From(string value) => new(value);
}
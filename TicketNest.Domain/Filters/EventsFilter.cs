namespace TicketNest.Domain.Filters;

public sealed class EventsFilter
{
    public string? Title { get; }

    public DateTime? From { get; }

    public DateTime? To { get; }

    public EventsFilter(string? title, DateTime? from, DateTime? to)
    {
        if (from != null && to != null)
        {
            Ensure.That(from >= to, $"from={from} <= to={to}");
        }

        Title = title;
        From = from;
        To = to;
    }
}
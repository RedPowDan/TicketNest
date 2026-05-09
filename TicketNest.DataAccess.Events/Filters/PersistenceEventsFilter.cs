using System.Linq.Expressions;
using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Filters;

namespace TicketNest.DataAccess.Events.Filters;

internal sealed class PersistenceEventsFilter
{
    public string? Title { get; }

    public DateTime? From { get; }

    public DateTime? To { get; }

    private PersistenceEventsFilter(string? title, DateTime? from, DateTime? to)
    {
        Title = title;
        From = from;
        To = to;
    }

    public static PersistenceEventsFilter CreateFrom(EventsFilter source)
    {
        return new PersistenceEventsFilter(
            title: source.Title,
            from: source.From,
            to: source.To);
    }

    public IEnumerable<Expression<Func<PersistenceEvent, bool>>> GetFilterExpressions()
    {
        if (Title != null)
        {
            yield return x => x.Title.Contains(Title, StringComparison.InvariantCultureIgnoreCase);
        }

        if (From.HasValue)
        {
            yield return x => x.StartAt >= From.Value;
        }

        if (To.HasValue)
        {
            yield return x => x.EndAt <= To.Value;
        }
    }
}
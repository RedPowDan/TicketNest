namespace TicketNest.Application.Events.Cache;

public static class CacheKeys
{
    public static string EventById(Guid id) => $"event:{id}";

    public const string TopEvents = "events:top10";

    public static readonly TimeSpan EventByIdTtl = TimeSpan.FromSeconds(60);

    public static readonly TimeSpan TopEventsTtl = TimeSpan.FromSeconds(30);
}

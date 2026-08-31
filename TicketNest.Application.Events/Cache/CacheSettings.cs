namespace TicketNest.Application.Events.Cache;

public sealed class CacheSettings
{
    public const string SectionName = "Cache";

    public string ConnectionString { get; init; } = "localhost:6379";

    public int EventByIdTtlSeconds { get; init; } = 60;

    public int TopEventsTtlSeconds { get; init; } = 30;

    public bool IsEnabled { get; init; } = true;
}

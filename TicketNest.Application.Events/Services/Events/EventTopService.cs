using Microsoft.Extensions.Logging;
using TicketNest.Application.Events.Cache;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Repositories;

namespace TicketNest.Application.Events.Services.Events;

internal sealed class EventTopService : IEventTopService
{
    private readonly IEventsRepository _eventsRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<EventTopService> _logger;

    public EventTopService(
        IEventsRepository eventsRepository,
        ICacheService cacheService,
        ILogger<EventTopService> logger)
    {
        _eventsRepository = eventsRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Event>> GetTop10(CancellationToken ct = default)
    {
        var cached = await _cacheService.GetAsync<IReadOnlyList<Event>>(CacheKeys.TopEvents, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for top-10 events");
            return cached;
        }

        _logger.LogDebug("Cache miss for top-10 events, querying database");
        var events = await _eventsRepository.GetTop10ByPopularity(ct);

        await _cacheService.SetAsync(CacheKeys.TopEvents, events, CacheKeys.TopEventsTtl, ct);

        return events;
    }
}

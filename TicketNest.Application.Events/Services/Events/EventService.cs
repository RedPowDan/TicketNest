using Microsoft.Extensions.Logging;
using TicketNest.Application.Events.Cache;
using TicketNest.Domain.Events.Constants;
using TicketNest.Domain.Events.Filters;
using TicketNest.Domain.Events.Models;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Pagination;
using TicketNest.Domain.Events.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Events.Services.Events;

internal sealed class EventService : IEventService
{
    private readonly IEventsRepository _eventsRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<EventService> _logger;

    public EventService(
        IEventsRepository eventsRepository,
        ICacheService cacheService,
        ILogger<EventService> logger)
    {
        _eventsRepository = eventsRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public Task<PaginatedResult<Event>> GetAll(EventsFilter filter, PaginationRequest paginationRequest, CancellationToken ct = default) =>
        _eventsRepository.GetAll(filter, paginationRequest, ct);

    public async Task<Event?> Get(Guid id, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.EventById(id);

        var cached = await _cacheService.GetAsync<Event>(cacheKey, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for event {EventId}", id);
            return cached;
        }

        _logger.LogDebug("Cache miss for event {EventId}, querying database", id);
        var @event = await _eventsRepository.Get(id, ct);

        if (@event is not null)
        {
            await _cacheService.SetAsync(cacheKey, @event, CacheKeys.EventByIdTtl, ct);
        }

        return @event;
    }

    public async Task<Result<Event, Error>> Create(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats,
        CancellationToken ct = default)
    {
        var eventCreateResult = Event.Create(title, description, startAt, endAt, totalSeats);
        if (eventCreateResult.IsFailure)
        {
            return new Error(ErrorCode.BadRequest, eventCreateResult.Error);
        }

        await _eventsRepository.Save(eventCreateResult.Value, ct);

        await _cacheService.RemoveAsync(CacheKeys.TopEvents, ct);

        return eventCreateResult.Value;
    }

    public async Task<UnitResult<Error>> Change(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        CancellationToken ct = default)
    {
        var eventModel = await _eventsRepository.Get(id, ct);
        if (eventModel == null)
        {
            return new Error(ErrorCode.NotFound, "Не найдено событие");
        }

        var changeTitleResult = eventModel.ChangeTitle(title);
        if (changeTitleResult.IsFailure)
        {
            return new Error(ErrorCode.BadRequest, changeTitleResult.Error);
        }

        var changeDescriptionResult = eventModel.ChangeDescription(description);
        if (changeDescriptionResult.IsFailure)
        {
            return new Error(ErrorCode.BadRequest, changeDescriptionResult.Error);
        }

        var changeStartAtAndEndAtResult = eventModel.ChangeStartAtAndEndAt(startAt, endAt);
        if (changeStartAtAndEndAtResult.IsFailure)
        {
            return new Error(ErrorCode.BadRequest, changeStartAtAndEndAtResult.Error);
        }

        await _eventsRepository.Save(eventModel, ct);

        await _cacheService.RemoveAsync(CacheKeys.EventById(id), ct);
        await _cacheService.RemoveAsync(CacheKeys.TopEvents, ct);

        return UnitResult<Error>.FromSuccess();
    }

    public async Task<UnitResult<Error>> Delete(Guid id, CancellationToken ct = default)
    {
        var isRemoved = await _eventsRepository.Remove(id, ct);
        if (!isRemoved)
        {
            return new Error(ErrorCode.NotFound, "Событие не найдено.");
        }

        await _cacheService.RemoveAsync(CacheKeys.EventById(id), ct);
        await _cacheService.RemoveAsync(CacheKeys.TopEvents, ct);

        return UnitResult<Error>.FromSuccess();
    }
}

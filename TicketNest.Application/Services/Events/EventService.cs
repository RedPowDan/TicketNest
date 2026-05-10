using TicketNest.Application.Constants;
using TicketNest.Application.Models;
using TicketNest.Domain.Filters;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Pagination;
using TicketNest.Domain.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Events;

internal sealed class EventService(IEventsRepository eventsRepository) : IEventService
{
    public Task<PaginatedResult<Event>> GetAll(EventsFilter filter, PaginationRequest paginationRequest, CancellationToken ct = default) =>
        eventsRepository.GetAll(filter, paginationRequest, ct);

    public async Task<Event?> Get(Guid id, CancellationToken ct = default) => await eventsRepository.Get(id, ct);

    public async Task<Result<Event, Error>> Create(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        CancellationToken ct = default)
    {
        var eventCreateResult = Event.Create(title, description, startAt, endAt);
        if (eventCreateResult.IsFailure)
        {
            return new Error(ErrorStatusCode.BadRequest, eventCreateResult.Error);
        }

        await eventsRepository.Save(eventCreateResult.Value, ct);

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
        var eventModel = await eventsRepository.Get(id, ct);
        if (eventModel == null)
        {
            return new Error(ErrorStatusCode.NotFound, "Не найдено событие");
        }

        var changeTitleResult = eventModel.ChangeTitle(title);
        if (changeTitleResult.IsFailure)
        {
            return new Error(ErrorStatusCode.BadRequest, changeTitleResult.Error);
        }

        var changeDescriptionResult = eventModel.ChangeDescription(description);
        if (changeDescriptionResult.IsFailure)
        {
            return new Error(ErrorStatusCode.BadRequest, changeDescriptionResult.Error);
        }

        var changeStartAtAndEndAtResult = eventModel.ChangeStartAtAndEndAt(startAt, endAt);
        if (changeStartAtAndEndAtResult.IsFailure)
        {
            return new Error(ErrorStatusCode.BadRequest, changeStartAtAndEndAtResult.Error);
        }

        await eventsRepository.Save(eventModel, ct);

        return UnitResult<Error>.FromSuccess();
    }

    public async Task<UnitResult<Error>> Delete(Guid id, CancellationToken ct = default)
    {
        var isRemoved = await eventsRepository.Remove(id, ct);
        if (!isRemoved)
        {
            return new Error(ErrorStatusCode.NotFound, "Событие не найдено.");
        }

        return UnitResult<Error>.FromSuccess();
    }
}
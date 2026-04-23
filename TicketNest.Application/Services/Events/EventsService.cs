using TicketNest.Application.Constants;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.ValueObjects;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Events;

public class EventsService(IEventsRepository eventsRepository)
{
    public Task<IReadOnlyCollection<Event>> GetAll(CancellationToken ct = default) => eventsRepository.GetAll(ct);

    public async Task<Event?> Get(EventId id, CancellationToken ct = default) => await eventsRepository.Get(id, ct);

    public async Task<Result<Event, string>> Create(
        EventTitle title,
        EventDescription? description,
        DateTime startAt,
        DateTime endAt,
        CancellationToken ct = default)
    {
        var eventCreateResult = Event.Create(title, description, startAt, endAt);
        if (eventCreateResult.IsFailure)
        {
            return eventCreateResult.Error;
        }

        await eventsRepository.Save(eventCreateResult.Value, ct);

        return eventCreateResult.Value;
    }

    public async Task<UnitResult<ErrorStatusCode>> Change(
        EventId id,
        EventTitle title,
        EventDescription? description,
        DateTime startAt,
        DateTime endAt,
        CancellationToken ct = default)
    {
        var eventModel = await eventsRepository.Get(id, ct);
        if (eventModel == null)
        {
            return ErrorStatusCode.NotFound;
        }

        var changeTitleResult = eventModel.ChangeTitle(title);
        if (changeTitleResult.IsFailure)
        {
            return ErrorStatusCode.BadRequest;
        }

        var changeDescriptionResult = eventModel.ChangeDescription(description);
        if (changeDescriptionResult.IsFailure)
        {
            return ErrorStatusCode.BadRequest;
        }

        var changeStartAtAndEndAtResult = eventModel.ChangeStartAtAndEndAt(startAt, endAt);
        if (changeStartAtAndEndAtResult.IsFailure)
        {
            return ErrorStatusCode.BadRequest;
        }

        await eventsRepository.Save(eventModel, ct);

        return UnitResult<ErrorStatusCode>.FromSuccess();
    }

    public async Task<UnitResult<ErrorStatusCode>> Delete(EventId id, CancellationToken ct = default)
    {
        var isRemoved = await eventsRepository.Remove(id, ct);
        if (!isRemoved)
        {
            return ErrorStatusCode.BadRequest;
        }

        return UnitResult<ErrorStatusCode>.FromSuccess();
    }
}
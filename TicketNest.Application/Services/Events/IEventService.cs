using TicketNest.Application.Constants;
using TicketNest.Application.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.ValueObjects;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Events;

public interface IEventService
{
    Task<IReadOnlyCollection<Event>> GetAll(CancellationToken ct = default);

    Task<Event?> Get(EventId id, CancellationToken ct = default);

    Task<Result<Event, Error>> Create(
        EventTitle title,
        EventDescription? description,
        DateTime startAt,
        DateTime endAt,
        CancellationToken ct = default);

    Task<UnitResult<Error>> Change(
        EventId id,
        EventTitle title,
        EventDescription? description,
        DateTime startAt,
        DateTime endAt,
        CancellationToken ct = default);

    Task<UnitResult<Error>> Delete(EventId id, CancellationToken ct = default);
}
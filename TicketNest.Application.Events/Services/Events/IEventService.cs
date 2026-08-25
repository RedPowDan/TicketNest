using TicketNest.Domain.Events.Filters;
using TicketNest.Domain.Events.Models;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Pagination;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Events.Services.Events;

public interface IEventService
{
    Task<PaginatedResult<Event>> GetAll(EventsFilter filter, PaginationRequest paginationRequest, CancellationToken ct = default);

    Task<Event?> Get(Guid id, CancellationToken ct = default);

    Task<Result<Event, Error>> Create(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats,
        CancellationToken ct = default);

    Task<UnitResult<Error>> Change(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        CancellationToken ct = default);

    Task<UnitResult<Error>> Delete(Guid id, CancellationToken ct = default);
}
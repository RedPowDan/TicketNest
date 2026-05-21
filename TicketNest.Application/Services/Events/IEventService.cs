using TicketNest.Domain.Filters;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Pagination;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Events;

public interface IEventService
{
    Task<PaginatedResult<Event>> GetAll(EventsFilter filter, PaginationRequest paginationRequest, CancellationToken ct = default);

    Task<Event?> Get(Guid id, CancellationToken ct = default);

    Task<Result<Event, Error>> Create(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
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
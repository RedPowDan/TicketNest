using TicketNest.Domain.Events.Filters;
using TicketNest.Domain.Events.Models;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Pagination;

namespace TicketNest.Domain.Events.Repositories;

public interface IEventsRepository
{
    Task Save(Event @event, CancellationToken ct = default);

    ValueTask<Event?> Get(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<Event>> GetAll(EventsFilter filter, PaginationRequest paginationRequest, CancellationToken ct = default);

    Task<bool> Remove(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Event>> GetTop10ByPopularity(CancellationToken ct = default);
}
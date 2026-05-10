using TicketNest.Domain.Filters;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Pagination;

namespace TicketNest.Domain.Repositories;

public interface IEventsRepository
{
    Task Save(Event @event, CancellationToken ct = default);

    ValueTask<Event?> Get(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<Event>> GetAll(EventsFilter filter, PaginationRequest paginationRequest, CancellationToken ct = default);

    Task<bool> Remove(Guid id, CancellationToken ct = default);
}
using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.DbContext;
using TicketNest.DataAccess.Events.Filters;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.Domain.Filters;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Pagination;
using TicketNest.Domain.Repositories;
using TicketNest.Shared.Expressions;

namespace TicketNest.DataAccess.Events.Implementations;

internal sealed class EventRepository(EventsDbContext dbContext) : IEventsRepository
{
    public async Task Save(Event @event, CancellationToken ct = default)
    {
        Ensure.NotNull(@event, nameof(@event));

        var persistenceEvent = await dbContext
            .Events
            .FindAsync([@event.Id], cancellationToken: ct);
        if (persistenceEvent != null)
        {
            EventMapper.Map(@event, persistenceEvent);
        }
        else
        {
            persistenceEvent = EventMapper.ToPersistence(@event);
            dbContext.Events.Add(persistenceEvent);
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public async ValueTask<Event?> Get(Guid id, CancellationToken ct = default)
    {
        var persistenceEvent = await dbContext
            .Events
            .FindAsync([id], cancellationToken: ct);

        return persistenceEvent == null
            ? null
            : EventMapper.ToDomain(persistenceEvent);
    }

    public async Task<PaginatedResult<Event>> GetAll(EventsFilter filter, PaginationRequest paginationRequest, CancellationToken ct = default)
    {
        var persistanceFilter = PersistenceEventsFilter.CreateFrom(filter);

        var expression = persistanceFilter.GetFilterExpressions().CombineAnd();

        var persistanceItems = await dbContext
            .Events
            .AsNoTracking()
            .Where(expression)
            .Skip((paginationRequest.Page - 1) * paginationRequest.PageSize)
            .Take(paginationRequest.PageSize)
            .ToArrayAsync(cancellationToken: ct);
        var items = persistanceItems.Select(EventMapper.ToDomain).ToArray();

        var totalCount = await dbContext
            .Events
            .AsNoTracking()
            .Where(expression)
            .CountAsync(cancellationToken: ct);

        return new PaginatedResult<Event>(items: items, totalCount: totalCount, currentPage: paginationRequest.Page);
    }

    public async Task<bool> Remove(Guid id, CancellationToken ct = default)
    {
        var persistenceEvent = await dbContext
            .Events
            .FindAsync([id], cancellationToken: ct);

        if (persistenceEvent == null)
            return false;

        dbContext.Events.Remove(persistenceEvent);
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
}
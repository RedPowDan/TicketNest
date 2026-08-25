using TicketNest.Domain.Events.Constants;
using TicketNest.Domain.Events.Models;
using TicketNest.Domain.Events.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Events.Services.Events;

public class EventReserveService : IEventReserveService
{
    private IEventsRepository _eventsRepository;

    public EventReserveService(IEventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    public async Task<UnitResult<Error>> Reserve(Guid eventId, DateTime reserveDateTime, CancellationToken ct)
    {
        var @event = await _eventsRepository.Get(eventId, ct);
        if (@event == null)
        {
            return new Error(ErrorCode.NotFound, $"Не найдено событие с Id:{eventId}");
        }

        var result = @event.TryReserveSeats(reserveDateTime);
        if (result.IsFailure)
        {
            return result;
        }

        await _eventsRepository.Save(@event, ct);

        return UnitResult<Error>.FromSuccess();
    }
}
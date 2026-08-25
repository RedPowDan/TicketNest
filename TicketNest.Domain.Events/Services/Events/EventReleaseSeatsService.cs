using TicketNest.Domain.Events.Constants;
using TicketNest.Domain.Events.Models;
using TicketNest.Domain.Events.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Events.Services.Events;

public sealed class EventReleaseSeatsService(IEventsRepository eventsRepository) : IEventReleaseSeatsService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <inheritdoc />
    public async Task<UnitResult<Error>> ReleaseSeats(Guid eventId, int count = 1, CancellationToken ct = default)
    {
        await Semaphore.WaitAsync(ct);
        try
        {
            var @event = await eventsRepository.Get(eventId, ct);
            if (@event is null)
            {
                return new Error(ErrorCode.NotFound, $"Ошибка отмены брони: событие {eventId} не найдено");
            }

            var isSuccess = @event.ReleaseSeats(count);
            if (!isSuccess)
            {
                return new Error(ErrorCode.BadRequest, "Невозможно вернуть бронь");
            }

            await eventsRepository.Save(@event, ct);
        }
        catch (Exception)
        {
            return new Error(ErrorCode.BadRequest, "Неизвестная ошибка отмены брони");
        }
        finally
        {
            Semaphore.Release();
        }

        return UnitResult<Error>.FromSuccess();
    }
}
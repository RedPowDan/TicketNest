using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Events;

public sealed class EventReleaseSeatsService(IEventsRepository eventsRepository, IBookingRepository bookingRepository) : IEventReleaseSeatsService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <inheritdoc />
    public async Task<UnitResult<Error>> ReleaseSeats(Guid bookingId, int count = 1, CancellationToken ct = default)
    {
        Ensure.NotDefault(bookingId, nameof(bookingId));

        var booking = await bookingRepository.Get(bookingId, ct);
        if (booking is null)
        {
            return new Error(ErrorCode.NotFound, $"Бронь {bookingId} не найдена");
        }

        await Semaphore.WaitAsync(ct);
        try
        {
            var @event = await eventsRepository.Get(booking.EventId, ct);
            if (@event is null)
            {
                return new Error(ErrorCode.NotFound, $"Ошибка отмены брони: событие {booking.EventId} не найдено");
            }

            var isSuccess = @event.ReleaseSeats(count);
            if (isSuccess)
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
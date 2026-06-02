using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Bookings;

public sealed class BookingConfirmationService(IBookingRepository bookingRepository, IEventsRepository eventsRepository) : IBookingConfirmationService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    public async Task<UnitResult<Error>> Confirm(Booking booking, CancellationToken ct)
    {
        await ConfirmInTicketSystem(booking, ct);

        booking.Confirm(processedAt: DateTime.UtcNow);

        return UnitResult<Error>.FromSuccess();
    }

    public async Task<UnitResult<Error>> Confirm(Guid bookingId, CancellationToken ct)
    {
        var booking = await bookingRepository.Get(bookingId, ct);
        if (booking == null)
        {
            return new Error(ErrorCode.NotFound, $"Бронь с Id={bookingId} не найдена");
        }

        var @event = await eventsRepository.Get(booking.EventId, ct);
        if (@event == null)
        {
            return new Error(ErrorCode.NotFound, $"Событие с Id={booking.EventId} не найдено");
        }

        await ConfirmInTicketSystem(booking, ct);

        await Semaphore.WaitAsync(ct);
        try
        {
            booking.Confirm(processedAt: DateTime.UtcNow);
            await bookingRepository.Save(booking, ct);
        }
        catch (Exception)
        {
            booking.Reject(processedAt: DateTime.UtcNow);
            @event.ReleaseSeats();

            await bookingRepository.Save(booking, CancellationToken.None);
            await eventsRepository.Save(@event, CancellationToken.None);

            return new Error(ErrorCode.BadRequest, "Ошибка бронирования");
        }
        finally
        {
            Semaphore.Release();
        }

        return UnitResult<Error>.FromSuccess();
    }

    /// <summary>
    /// При подключении билетной системы, будем вызывать ее тут, а пока что просто останавливаем выполнение на несколько секунд
    /// </summary>
    private async Task ConfirmInTicketSystem(Booking booking, CancellationToken ct)
    {
        await Task.Delay(1000, ct);
    }
}
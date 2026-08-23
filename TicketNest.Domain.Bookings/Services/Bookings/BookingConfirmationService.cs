using TicketNest.Domain.Bookings.Constants;
using TicketNest.Domain.Bookings.Models;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Bookings.Services.Bookings;

public sealed class BookingConfirmationService(IBookingRepository bookingRepository) : IBookingConfirmationService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public async Task<UnitResult<Error>> Confirm(Guid bookingId, CancellationToken ct)
    {
        var booking = await bookingRepository.Get(bookingId, ct);
        if (booking == null)
        {
            return new Error(ErrorCode.NotFound, $"Бронь с Id={bookingId} не найдена");
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
            
            await bookingRepository.Save(booking, CancellationToken.None);

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
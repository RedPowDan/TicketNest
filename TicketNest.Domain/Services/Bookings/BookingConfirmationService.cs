using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Bookings;

public sealed class BookingConfirmationService : IBookingConfirmationService
{
    public async Task<UnitResult<Error>> Confirm(Booking booking, CancellationToken ct)
    {
        await ConfirmInTicketSystem(booking, ct);

        booking.Confirm(processedAt: DateTime.UtcNow);

        return UnitResult<Error>.FromSuccess();
    }

    /// <summary>
    /// При подключении билетной системы, будем вызывать ее тут, а пока что просто останавливаем выполнение на несколько секунд
    /// </summary>
    private async Task ConfirmInTicketSystem(Booking booking, CancellationToken ct)
    {
        var random = new Random(DateTime.Now.Millisecond);
        var milliseconds = (int) Math.Abs(random.NextInt64()) % 10_000; // До 10 секунд
        await Task.Delay(milliseconds, ct);
    }
}
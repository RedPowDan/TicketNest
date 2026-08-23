namespace TicketNest.Application.Bookings.Services.Outbox;

public interface IOutboxProcessingService
{
    Task ProcessPendingAsync(CancellationToken ct = default);
}
using TicketNest.Contracts.Kafka.Messages;

namespace TicketNest.Application.Bookings.Services;

public interface IBookingsConsumer
{
    Task HandleBookingApprovedMessage(Func<BookingApprovedMessage, CancellationToken, Task> func, CancellationToken ct);
}
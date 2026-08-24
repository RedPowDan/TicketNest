using TicketNest.Contracts.Kafka.Messages;

namespace TicketNest.Application.Events.Services;

public interface IEventsConsumer
{
    Task HandleBookingCreatedMessage(Func<BookingCreatedMessage, CancellationToken, Task> func, CancellationToken ct);
}
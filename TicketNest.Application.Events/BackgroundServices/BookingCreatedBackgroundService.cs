using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketNest.Application.Events.Services;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Events.Services;
using TicketNest.Domain.Events.Services.Events;

namespace TicketNest.Application.Events.BackgroundServices;

internal sealed class BookingCreatedBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BookingCreatedBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IEventsConsumer>();
        await consumer.HandleBookingCreatedMessage(HandleMessage, ct);
    }

    private async Task HandleMessage(BookingCreatedMessage message, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var eventReserveService = scope.ServiceProvider.GetRequiredService<IEventReserveService>();
        var eventsProducer = scope.ServiceProvider.GetRequiredService<IEventsProducer>();

        var result = await eventReserveService.Reserve(eventId: message.EventId, reserveDateTime: DateTime.UtcNow, ct);
        if (result.IsFailure)
        {
            await eventsProducer.BookingRejected(
                bookingId: message.BookingId,
                eventId: message.EventId,
                reason: result.Error.Message,
                CancellationToken.None);
            return;
        }

        await eventsProducer.BookingApproved(
            bookingId: message.BookingId,
            eventId: message.EventId,
            CancellationToken.None);
    }
}

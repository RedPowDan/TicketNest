using Microsoft.Extensions.Hosting;
using TicketNest.Application.Events.Services;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Events.Services;
using TicketNest.Domain.Events.Services.Events;

namespace TicketNest.Application.Events.BackgroundServices;

internal sealed class BookingCreatedBackgroundService : BackgroundService
{
    private readonly IEventReserveService _eventReserveService;
    private readonly IEventsProducer _eventsProducer;
    private readonly IEventsConsumer _eventsConsumer;

    public BookingCreatedBackgroundService(
        IEventReserveService eventReserveService,
        IEventsProducer eventsProducer,
        IEventsConsumer eventsConsumer)
    {
        _eventReserveService = eventReserveService;
        _eventsProducer = eventsProducer;
        _eventsConsumer = eventsConsumer;
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        return _eventsConsumer.HandleBookingCreatedMessage(HandleMessage, ct);
    }

    private async Task HandleMessage(BookingCreatedMessage message, CancellationToken ct)
    {
        var result = await _eventReserveService.Reserve(eventId: message.EventId, reserveDateTime: DateTime.UtcNow, ct);
        if (result.IsFailure)
        {
            await _eventsProducer.BookingRejected(
                bookingId: message.BookingId,
                eventId: message.EventId,
                reason: result.Error.Message,
                CancellationToken.None);
            return;
        }

        await _eventsProducer.BookingApproved(
            bookingId: message.BookingId,
            eventId: message.EventId,
            CancellationToken.None);
    }
}
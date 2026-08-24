using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketNest.Application.Events.Services;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Events.Services.Events;

namespace TicketNest.Application.Events.BackgroundServices;

internal sealed class BookingCancelledBackgroundService : BackgroundService
{
    private readonly IEventsConsumer _eventsConsumer;
    private readonly IEventReleaseSeatsService _eventReleaseSeatsService;
    private readonly ILogger<BookingCancelledBackgroundService> _logger;

    public BookingCancelledBackgroundService(
        IEventsConsumer eventsConsumer,
        IEventReleaseSeatsService eventReleaseSeatsService,
        ILogger<BookingCancelledBackgroundService> logger)
    {
        _eventsConsumer = eventsConsumer;
        _eventReleaseSeatsService = eventReleaseSeatsService;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        return _eventsConsumer.HandleBookingCancelledMessage(HandleMessage, ct);
    }

    private async Task HandleMessage(BookingCancelledMessage message, CancellationToken ct)
    {
        var result = await _eventReleaseSeatsService.ReleaseSeats(message.EventId, ct: ct);
        if (result.IsFailure)
        {
            _logger.LogError("Произошла ошибка при обработке сообщения об отмене бронирования.: {@Message}", message);
        }

        _logger.LogInformation("Освобождено место для события {MessageEventId}. Бронь {MessageBookingId}", message.EventId, message.BookingId);
    }
}
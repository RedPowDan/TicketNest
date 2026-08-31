using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketNest.Application.Events.Cache;
using TicketNest.Application.Events.Services;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Events.Services.Events;

namespace TicketNest.Application.Events.BackgroundServices;

internal sealed class BookingCancelledBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingCancelledBackgroundService> _logger;

    public BookingCancelledBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingCancelledBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IEventsConsumer>();
        await consumer.HandleBookingCancelledMessage(HandleMessage, ct);
    }

    private async Task HandleMessage(BookingCancelledMessage message, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var eventReleaseSeatsService = scope.ServiceProvider.GetRequiredService<IEventReleaseSeatsService>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var result = await eventReleaseSeatsService.ReleaseSeats(message.EventId, ct: ct);
        if (result.IsFailure)
        {
            _logger.LogError("Произошла ошибка при обработке сообщения об отмене бронирования.: {@Message}", message);
        }

        await cacheService.RemoveAsync(CacheKeys.EventById(message.EventId), CancellationToken.None);
        await cacheService.RemoveAsync(CacheKeys.TopEvents, CancellationToken.None);

        _logger.LogInformation("Освобождено место для события {MessageEventId}. Бронь {MessageBookingId}", message.EventId, message.BookingId);
    }
}

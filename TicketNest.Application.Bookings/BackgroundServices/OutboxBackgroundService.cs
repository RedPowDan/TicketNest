using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketNest.Application.Bookings.Services.Outbox;
using TicketNest.Domain.Bookings.Outbox;

namespace TicketNest.Application.Bookings.BackgroundServices;

/// <summary>
/// Фоновый сервис слоя Application: периодически вызывает <see cref="IOutboxProcessingService"/>
/// для последовательной обработки сообщений Outbox. Для каждой итерации создаёт свой scope.
/// </summary>
public sealed class OutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(5);

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessingService>();
                await processor.ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка фоновой обработки Outbox");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

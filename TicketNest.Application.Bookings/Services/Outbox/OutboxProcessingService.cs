using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Outbox;
using TicketNest.Domain.Bookings.Repositories;

namespace TicketNest.Application.Bookings.Services.Outbox;

/// <summary>
/// Оркестрирующий сервис:
/// 1. получает доменные сообщения Outbox из репозитория (Id + готовое доменное событие);
/// 2. для каждого сообщения последовательно вызывает все зарегистрированные обработчики
///    <see cref="IEventHandler{TEvent}"/> для конкретного типа события (резолв через DI);
/// 3. помечает сообщение обработанным или упавшим.
/// </summary>
internal sealed class OutboxProcessingService(
    IOutboxRepository outboxRepository,
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessingService> logger) : IOutboxProcessingService
{
    private const int BatchSize = 20;

    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        // 1. Получаем доменные сообщения (Id + готовое доменное событие).
        var messages = await outboxRepository.GetPendingAsync(BatchSize, ct);

        // 2. Обрабатываем строго последовательно, по одному сообщению.
        foreach (var message in messages)
        {
            try
            {
                var eventType = message.Event.GetType();
                var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                var handleMethod = handlerType.GetMethod("HandleAsync")
                    ?? throw new InvalidOperationException($"Метод HandleAsync не найден у {handlerType.Name}");

                var handlers = serviceProvider.GetServices(handlerType).ToList();

                if (handlers.Count == 0)
                {
                    logger.LogWarning(
                        "Для события {EventType} (сообщение {MessageId}) не зарегистрировано обработчиков Outbox",
                        eventType.Name,
                        message.Id);
                }
                else
                {
                    foreach (var handler in handlers)
                    {
                        await (Task)handleMethod.Invoke(handler, new object[] { message.Event, ct })!;
                    }
                }

                await outboxRepository.MarkProcessedAsync(message.Id, ct);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Ошибка обработки Outbox-сообщения {MessageId}",
                    message.Id);

                await outboxRepository.MarkFailedAsync(message.Id, exception.Message, ct);
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;

namespace TicketNest.Application.BackgroundServices;

public class BookingCancellationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingCancellationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (ct.IsCancellationRequested == false)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var queueMessageRepository = scope.ServiceProvider.GetRequiredService<IQueueMessageRepository>();
                var messages = await queueMessageRepository.GetAll<BookingCanceledMessage>(queueName: QueueNames.BookingQueue, ct);
                if (!messages.Any())
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    continue;
                }

                var tasks = messages.Select(message => HandleMessage(message, queueMessageRepository, ct));

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка обработки сообщения отмены бронирования");
            }
        }
    }

    private async Task HandleMessage(
        QueueMessage<BookingCanceledMessage> message,
        IQueueMessageRepository queueMessageRepository,
        CancellationToken ct)
    {
        using var messageScope = scopeFactory.CreateScope();
        var eventsRepository = messageScope.ServiceProvider.GetRequiredService<IEventsRepository>();
        var bookingRepository = messageScope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var bookingId = message.Data.BookingId;
        var booking = await bookingRepository.Get(bookingId, ct);
        if (booking is null)
        {
            logger.LogError("Ошибка отмены брони: бронь {BookingId} не найдена", bookingId);
            return;
        }

        var @event = await eventsRepository.Get(booking.EventId, ct);
        if (@event is null)
        {
            logger.LogError("Ошибка отмены брони: событие {BookingEventId} не найдено", booking.EventId);
            return;
        }

        var isSuccess = @event.ReleaseSeats();
        if (isSuccess)
        {
            logger.LogError("Невозможно вернуть место");
            return;
        }

        await queueMessageRepository.Commit(message.MessageId, ct);

        logger.LogTrace("Бронь с идентификатором {BookingId} отменена", message.Data.BookingId);
    }
}
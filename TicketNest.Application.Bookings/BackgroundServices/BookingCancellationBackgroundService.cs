using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketNest.Domain.Bookings.Constants;
using TicketNest.Shared;

namespace TicketNest.Application.Bookings.BackgroundServices;

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
                // using var scope = scopeFactory.CreateScope();
                // var queueMessageRepository = scope.ServiceProvider.GetRequiredService<IQueueMessageRepository>();
                // var messages = await queueMessageRepository.GetAll<BookingCanceledMessage>(queueName: QueueNames.BookingCancelledQueue, ct);
                // if (!messages.Any())
                // {
                //     await Task.Delay(TimeSpan.FromSeconds(1), ct);
                //     continue;
                // }
                //
                // var tasks = messages.Select(message => HandleMessage(message, queueMessageRepository, ct));
                //
                // await Task.WhenAll(tasks);
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

    // internal async Task HandleMessage(
    //     QueueMessage<BookingCanceledMessage> message,
    //     IQueueMessageRepository queueMessageRepository,
    //     CancellationToken ct)
    // {
    //     using var messageScope = scopeFactory.CreateScope();
    //     var eventReleaseSeatsService = messageScope.ServiceProvider.GetRequiredService<IEventReleaseSeatsService>();
    //
    //     var bookingId = message.Data.BookingId;
    //     var result = await eventReleaseSeatsService.ReleaseSeats(bookingId, ct: ct);
    //     if (result.IsFailure)
    //     {
    //         logger.LogError("Ошибка отмены брони {BookingId}. Подробности: {@ResultError}", bookingId, result.Error);
    //     }
    //
    //     await queueMessageRepository.Commit(message.MessageId, ct);
    //
    //     logger.LogTrace("Бронь с идентификатором {BookingId} отменена", bookingId);
    // }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Bookings;

namespace TicketNest.Application.BackgroundServices;

public class BookingConfirmationBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<BookingConfirmationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (ct.IsCancellationRequested == false)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var queueMessageRepository = scope.ServiceProvider.GetRequiredService<IQueueMessageRepository>();
                var messages = await queueMessageRepository.GetAll<BookingCreatedMessage>(queueName: QueueNames.BookingQueue, ct);
                if (!messages.Any())
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    continue;
                }

                var bookingConfirmationService = scope.ServiceProvider.GetRequiredService<IBookingConfirmationService>();

                var tasks = messages.Select(message => HandleMessage(message, bookingConfirmationService, queueMessageRepository, ct));

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка обработки сообщения подтверждения бронирования");
            }
        }
    }

    internal async Task HandleMessage(
        QueueMessage<BookingCreatedMessage> message,
        IBookingConfirmationService bookingConfirmationService,
        IQueueMessageRepository queueMessageRepository,
        CancellationToken ct)
    {
        var confirmationResult = await bookingConfirmationService.Confirm(message.Data.BookingId, ct);
        if (confirmationResult.IsFailure)
        {
            logger.LogError("Ошибка подтверждения брони: {Message}", confirmationResult.Error.Message);
            return;
        }

        await queueMessageRepository.Commit(message.MessageId, ct);

        logger.LogTrace("Бронь с идентификатором {BookingId} подтверждена", message.Data.BookingId);
    }
}
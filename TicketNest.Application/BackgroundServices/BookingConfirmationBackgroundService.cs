using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.BackgroundServices;

public class BookingConfirmationBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<BookingConfirmationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (ct.IsCancellationRequested == false)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var queueMessageRepository = scope.ServiceProvider.GetRequiredService<IQueueMessageRepository>();

                var message = await queueMessageRepository.Get<BookingCreatedMessage>(queueName: QueueNames.BookingQueue, ct);
                if (message == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    continue;
                }

                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var bookingConfirmationService = scope.ServiceProvider.GetRequiredService<IBookingConfirmationService>();
                var result = await HandleMessage(message.Data, bookingRepository, bookingConfirmationService, ct);
                if (result.IsFailure)
                {
                    logger.LogError(result.Error);
                    continue;
                }

                await queueMessageRepository.Commit(message.MessageId, ct);

                logger.LogTrace("Бронь с идентификатором {0} подтверждена", message.Data.BookingId);
            }
        }
    }

    private async Task<UnitResult<string>> HandleMessage(
        BookingCreatedMessage message,
        IBookingRepository bookingRepository,
        IBookingConfirmationService bookingConfirmationService,
        CancellationToken ct)
    {
        var booking = await bookingRepository.Get(message.BookingId, ct);
        if (booking == null)
        {
            return $"Бронь с идентификатором {message.BookingId} не найдена";
        }

        var confirmationResult = await bookingConfirmationService.Confirm(booking, ct);
        if (confirmationResult.IsFailure)
        {
            return confirmationResult.Error.Message;
        }

        await bookingRepository.Save(booking, ct);

        return UnitResult<string>.FromSuccess();
    }
}
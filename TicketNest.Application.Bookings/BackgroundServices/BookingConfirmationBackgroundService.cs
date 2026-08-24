using Microsoft.Extensions.Hosting;
using TicketNest.Application.Bookings.Services;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Bookings.Services.Bookings;


namespace TicketNest.Application.Bookings.BackgroundServices;

public class BookingConfirmationBackgroundService(IBookingsConsumer consumer, IBookingConfirmationService bookingConfirmationService) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct)
    {
        return consumer.HandleBookingApprovedMessage(HandleMessage, ct);
    }

    private Task HandleMessage(BookingApprovedMessage message, CancellationToken ct)
    {
        return bookingConfirmationService.Confirm(message.BookingId, ct);
    }
}
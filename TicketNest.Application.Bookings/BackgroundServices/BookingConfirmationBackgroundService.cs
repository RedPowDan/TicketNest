using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketNest.Application.Bookings.Services;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Bookings.Services.Bookings;

namespace TicketNest.Application.Bookings.BackgroundServices;

internal sealed class BookingConfirmationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BookingConfirmationBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IBookingsConsumer>();
        await consumer.HandleBookingApprovedMessage(HandleMessage, ct);
    }

    private Task HandleMessage(BookingApprovedMessage message, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingConfirmationService = scope.ServiceProvider.GetRequiredService<IBookingConfirmationService>();
        return bookingConfirmationService.Confirm(message.BookingId, ct);
    }
}

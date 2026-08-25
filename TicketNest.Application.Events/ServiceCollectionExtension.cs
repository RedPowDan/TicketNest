using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.Events.BackgroundServices;
using TicketNest.Application.Events.Services.Events;
using TicketNest.Domain.Events.Services.Events;

namespace TicketNest.Application.Events;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
                .AddScoped<IEventService, EventService>()
                .AddScoped<IEventReserveService, EventReserveService>()
                .AddScoped<IEventReleaseSeatsService, EventReleaseSeatsService>()
                .AddHostedService<BookingCancelledBackgroundService>()
                .AddHostedService<BookingCreatedBackgroundService>()
            ;
    }
}
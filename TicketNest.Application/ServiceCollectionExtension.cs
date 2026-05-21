using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.BackgroundServices;
using TicketNest.Application.Services.Bookings;
using TicketNest.Application.Services.Events;
using TicketNest.Domain.Services.Bookings;

namespace TicketNest.Application;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
                .AddDomainServices()
                .AddHostedServices()
                .AddScoped<IEventService, EventService>()
                .AddScoped<IBookingService, BookingService>()
            ;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        return services
                .AddScoped<IBookingFactory, BookingFactory>()
                .AddScoped<IBookingConfirmationService, BookingConfirmationService>()
            ;
    }

    private static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        return services
                .AddHostedService<BookingConfirmationBackgroundService>()
            ;
    }
}
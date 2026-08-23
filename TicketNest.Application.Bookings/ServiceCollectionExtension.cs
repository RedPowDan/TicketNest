using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.Bookings.BackgroundServices;
using TicketNest.Application.Bookings.Services.Bookings;
using TicketNest.Domain.Bookings.Services.Bookings;

namespace TicketNest.Application.Bookings;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
                .AddDomainServices()
                .AddHostedServices()
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
                .AddHostedService<BookingCancellationBackgroundService>()
            ;
    }
}
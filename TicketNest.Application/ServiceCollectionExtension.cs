using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.BackgroundServices;
using TicketNest.Application.Services.Bookings;
using TicketNest.Application.Services.Events;
using TicketNest.Domain.Services.Bookings;
using TicketNest.Domain.Services.Events;
using TicketNest.Domain.Services.Users;

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
                .AddScoped<IEventReleaseSeatsService, EventReleaseSeatsService>()
                .AddScoped<IUserFactory, UserFactory>()
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
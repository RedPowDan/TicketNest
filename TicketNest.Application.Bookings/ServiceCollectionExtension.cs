using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.Bookings.BackgroundServices;
using TicketNest.Application.Bookings.Services.Bookings;
using TicketNest.Application.Bookings.Services.Outbox;
using TicketNest.Domain.Bookings.Outbox;
using TicketNest.Domain.Bookings.Services.Bookings;

namespace TicketNest.Application.Bookings;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
                .AddDomainServices()
                .AddHostedServices()
                .AddOutboxHandlers()
                .AddScoped<IOutboxProcessingService, OutboxProcessingService>()
                .AddHostedService<OutboxBackgroundService>()
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

    /// <summary>
    /// Автоматическая регистрация всех обработчиков <see cref="IEventHandler{TEvent}"/> из слоя Application
    /// по закрытому generic-типу события. На один тип события можно зарегистрировать несколько обработчиков.
    /// </summary>
    private static IServiceCollection AddOutboxHandlers(this IServiceCollection services)
    {
        var handlerDescriptors = typeof(ServiceCollectionExtension).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Select(t => new
            {
                Type = t,
                Iface = t.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)),
            })
            .Where(x => x.Iface is not null)
            .ToList();

        foreach (var descriptor in handlerDescriptors)
        {
            var eventType = descriptor.Iface!.GetGenericArguments()[0];
            var handlerInterface = typeof(IEventHandler<>).MakeGenericType(eventType);

            services.AddScoped(handlerInterface, descriptor.Type);
            services.AddScoped(descriptor.Type);
        }

        return services;
    }
}

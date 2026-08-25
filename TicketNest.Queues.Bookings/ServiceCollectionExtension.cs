using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.Bookings.Services;
using TicketNest.Domain.Bookings.Services;
using TicketNest.Queues.Bookings.Settings;

namespace TicketNest.Queues.Bookings;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddQueues(this IServiceCollection services, IConfiguration configuration)
    {
        return services
                .AddScoped<IBookingProducer, BookingProducer>()
                .AddScoped<IBookingsConsumer, BookingsConsumer>()
                .AddKafkaSettings(configuration)
            ;
    }

    private static IServiceCollection AddKafkaSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaSection = configuration.GetSection("Kafka");

        var baseUrl = kafkaSection["BaseUrl"];
        var login = kafkaSection["Login"];
        var password = kafkaSection["Password"];

        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException("Kafka BaseUrl is not configured");
        }

        var kafkaSettings = new BookingKafkaSettings(baseUrl, login, password);
        services.AddSingleton(kafkaSettings);

        return services;
    }
}
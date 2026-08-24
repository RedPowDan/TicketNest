using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.Events.Services;
using TicketNest.Domain.Events.Services;
using TicketNest.Queues.Events.Settings;

namespace TicketNest.Queues.Events;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddQueues(this IServiceCollection services, IConfiguration configuration)
    {
        return services
                .AddScoped<IEventsProducer, EventsProducer>()
                .AddScoped<IEventsConsumer, EventsConsumer>()
                .AddKafkaSettings(configuration)
            ;
    }

    private static IServiceCollection AddKafkaSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaSection = configuration.GetSection("Kafka");

        var baseUrl = kafkaSection["BaseUrl"];
        var login = kafkaSection["Login"];
        var password = kafkaSection["Password"];

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("Kafka settings are not configured properly");
        }

        var kafkaSettings = new EventsKafkaSettings(baseUrl, login, password);
        services.AddSingleton(kafkaSettings);

        return services;
    }
}
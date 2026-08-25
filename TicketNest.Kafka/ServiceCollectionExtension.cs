using Microsoft.Extensions.DependencyInjection;
using TicketNest.Kafka.Consumer;
using TicketNest.Kafka.Producer;

namespace TicketNest.Kafka;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddKafkaInfrastructure(this IServiceCollection services)
    {
        return services
            .AddScoped<IKafkaConsumerGatewayFactory, KafkaConsumerGatewayFactory>()
            .AddScoped<IKafkaProducerGatewayFactory, KafkaProducerGatewayFactory>()
            ;
    }
}

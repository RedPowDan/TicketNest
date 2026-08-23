using Microsoft.Extensions.DependencyInjection;
using TicketNest.Kafka.Consumer;
using TicketNest.Kafka.Producer;

namespace TicketNest.Kafka;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services
            .AddScoped(typeof(IKafkaConsumerGatewayFactory<>), typeof(KafkaConsumerGatewayFactory<>))
            .AddScoped<IKafkaProducerGatewayFactory, KafkaProducerGatewayFactory>()
            ;
    }
}

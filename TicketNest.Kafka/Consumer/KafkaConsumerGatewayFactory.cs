using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace TicketNest.Kafka.Consumer;

internal sealed class KafkaConsumerGatewayFactory : IKafkaConsumerGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    public KafkaConsumerGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IKafkaConsumerGateway<T> CreateGateway<T>(
        ConsumerConfig config, string topic,
        Func<IncomingMessage<T>, CancellationToken, Task> messageHandler) where T : class
    {
        var logger = (ILogger<KafkaConsumerGateway<T>>) _serviceProvider.GetService(typeof(ILogger<KafkaConsumerGateway<T>>))!;
        return new KafkaConsumerGateway<T>(config, topic, logger, messageHandler);
    }
}
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace TicketNest.Kafka.Consumer;

internal sealed class KafkaConsumerGatewayFactory<T> : IKafkaConsumerGatewayFactory<T> where T : class
{
    private readonly ILogger<KafkaConsumerGateway<T>> _logger;

    public KafkaConsumerGatewayFactory(ILogger<KafkaConsumerGateway<T>> logger)
    {
        _logger = logger;
    }

    public IKafkaConsumerGateway<T> CreateGateway(
        ConsumerConfig config,
        string topic,
        Func<IncomingMessage<T>, CancellationToken, Task> messageHandler)
    {
        return new KafkaConsumerGateway<T>(config, topic, _logger, messageHandler);
    }
}
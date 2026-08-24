using Confluent.Kafka;

namespace TicketNest.Kafka.Consumer;

public interface IKafkaConsumerGatewayFactory
{
    public IKafkaConsumerGateway<T> CreateGateway<T>(
        ConsumerConfig config,
        string topic,
        Func<IncomingMessage<T>, CancellationToken, Task> messageHandler) where T : class;
}
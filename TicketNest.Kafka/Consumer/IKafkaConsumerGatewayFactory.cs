using Confluent.Kafka;

namespace TicketNest.Kafka.Consumer;

public interface IKafkaConsumerGatewayFactory<T> where T : class
{
    public IKafkaConsumerGateway<T> CreateGateway(
        ConsumerConfig config,
        string topic,
        Func<IncomingMessage<T>, CancellationToken, Task> messageHandler);
}
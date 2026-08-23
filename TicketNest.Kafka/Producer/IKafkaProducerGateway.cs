using Confluent.Kafka;

namespace TicketNest.Kafka.Producer;

public interface IKafkaProducerGateway : IDisposable
{
    Task Produce(Message<string, string> outgoingMessage, string topic, CancellationToken ct);
}
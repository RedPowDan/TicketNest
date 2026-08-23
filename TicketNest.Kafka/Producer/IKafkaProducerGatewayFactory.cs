using Confluent.Kafka;

namespace TicketNest.Kafka.Producer;

public interface IKafkaProducerGatewayFactory
{
    IKafkaProducerGateway Create(ProducerConfig config);
}
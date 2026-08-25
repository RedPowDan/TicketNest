using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace TicketNest.Kafka.Producer;

internal sealed class KafkaProducerGatewayFactory : IKafkaProducerGatewayFactory
{
    private readonly ILogger<KafkaProducerGateway> _logger;

    public KafkaProducerGatewayFactory(ILogger<KafkaProducerGateway> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IKafkaProducerGateway Create(ProducerConfig config)
    {
        return new KafkaProducerGateway(_logger, config);
    }
}
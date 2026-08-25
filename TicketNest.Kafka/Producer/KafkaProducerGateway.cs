using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace TicketNest.Kafka.Producer;

internal sealed class KafkaProducerGateway : IKafkaProducerGateway
{
    private readonly ILogger<KafkaProducerGateway> _logger;
    private readonly IProducer<string, string> _producer;

    public KafkaProducerGateway(ILogger<KafkaProducerGateway> logger, ProducerConfig config)
    {
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <inheritdoc />
    public async Task Produce(Message<string, string> outgoingMessage, string topic, CancellationToken ct)
    {
        try
        {
            var result = await _producer.ProduceAsync(topic, outgoingMessage, ct);

            _logger.LogInformation(
                "Отправлено сообщение. Key:{Key}, Message:{Message}, Topic: {Topic}, Partition: {Partition}",
                outgoingMessage.Key,
                outgoingMessage.Value,
                topic,
                result.Partition.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка получения сообщения. Topic: {Topic}", topic);
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace TicketNest.Kafka.Consumer;

internal sealed class KafkaConsumerGateway<T> : IKafkaConsumerGateway<T> where T : class
{
    private readonly ILogger<KafkaConsumerGateway<T>> _logger;
    private readonly string _topic;
    private readonly Func<IncomingMessage<T>, CancellationToken, Task> _messageHandler;
    private readonly IConsumer<Ignore, string> _consumer;

    public KafkaConsumerGateway(
        ConsumerConfig config,
        string topic,
        ILogger<KafkaConsumerGateway<T>> logger,
        Func<IncomingMessage<T>, CancellationToken, Task> messageHandler)
    {
        _topic = topic;
        _logger = logger;
        _messageHandler = messageHandler;

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(topic);
    }

    public async Task Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await ProcessMessage(ct);
        }
    }

    private async Task ProcessMessage(CancellationToken ct)
    {
        var consumeResult = default(ConsumeResult<Ignore, string>?);

        try
        {
            consumeResult = _consumer.Consume(KafkaConsumerGatewaySettings.ConsumeTimeout);

            if (consumeResult?.IsPartitionEOF != false)
            {
                await Task.Delay(KafkaConsumerGatewaySettings.RetryDelayWhenEmptyTopic, ct);
                return;
            }

            LogMessage(consumeResult);

            var content = GetContent(consumeResult);

            var message = new IncomingMessage<T>(
                content,
                consumeResult.Partition.Value,
                consumeResult.Offset.Value,
                consumeResult.Message.Timestamp.UtcDateTime);

            await _messageHandler(message, ct);
        }
        catch (Exception e)
        {
            LogError(consumeResult?.Message.Value, e);
        }

        if (consumeResult != null && !ct.IsCancellationRequested)
        {
            _consumer.Commit(consumeResult);
        }
    }

    private static T GetContent(ConsumeResult<Ignore, string> consumeResult)
    {
        var content = JsonSerializer.Deserialize<T>(consumeResult.Message.Value, KafkaConsumerGatewaySettings.JsonSerializerOptions);
        if (content == null)
        {
            throw new InvalidOperationException("Пустое сообщение.");
        }

        return content;
    }

    private void LogMessage(ConsumeResult<Ignore, string> consumeResult)
    {
        _logger.LogTrace(
            "Получено сообщение: {Message}, topic: {Topic}, partition: {Partition}",
            consumeResult.Message.Value,
            consumeResult.Topic,
            consumeResult.Partition.Value);
    }

    private void LogError(string? message, Exception ex)
    {
        _logger.LogError(ex, "Ошибка обработки сообщения: {Message}. Topic: {Topic}", message, _topic);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _consumer.Dispose();
    }
}
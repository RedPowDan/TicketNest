using System.Text.Json;
using Confluent.Kafka;
using TicketNest.Contracts.Kafka;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Events.Services;
using TicketNest.Kafka.Producer;
using TicketNest.Queues.Events.Settings;

namespace TicketNest.Queues.Events;

internal sealed class EventsProducer : IEventsProducer
{
    private readonly IKafkaProducerGatewayFactory _kafkaProducerGatewayFactory;
    private readonly EventsKafkaSettings _settings;

    public EventsProducer(IKafkaProducerGatewayFactory kafkaProducerGatewayFactory, EventsKafkaSettings settings)
    {
        _kafkaProducerGatewayFactory = kafkaProducerGatewayFactory;
        _settings = settings;
    }

    public async Task BookingRejected(Guid bookingId, Guid eventId, string reason, CancellationToken ct)
    {
        var message = new BookingRejectedMessage(bookingId, eventId, reason);
        var outgoingMessage = CreateMessage(bookingId, message);

        var gateway = CreateGateway();
        await gateway.Produce(
            outgoingMessage: outgoingMessage,
            topic: KafkaTopics.BookingTopic,
            ct);
    }

    public async Task BookingApproved(Guid bookingId, Guid eventId, CancellationToken ct)
    {
        var message = new BookingApprovedMessage(bookingId, eventId);
        var outgoingMessage = CreateMessage(bookingId, message);

        var gateway = CreateGateway();
        await gateway.Produce(
            outgoingMessage: outgoingMessage,
            topic: KafkaTopics.EventTopic,
            ct);
    }

    private static Message<string, string> CreateMessage<T>(Guid id, T message)
    {
        return new Message<string, string>
        {
            Key = id.ToString(),
            Value = JsonSerializer.Serialize(message)
        };
    }

    private IKafkaProducerGateway CreateGateway()
    {
        return _kafkaProducerGatewayFactory.Create(new ProducerConfig
        {
            BootstrapServers = _settings.BaseUrl,
            SaslUsername = _settings.Login,
            SaslPassword = _settings.Password
        });
    }
}
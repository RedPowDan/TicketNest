using Confluent.Kafka;
using TicketNest.Application.Events.Services;
using TicketNest.Contracts.Kafka;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Kafka.Consumer;
using TicketNest.Queues.Events.Settings;

namespace TicketNest.Queues.Events;

internal sealed class EventsConsumer : IEventsConsumer
{
    private readonly IKafkaConsumerGatewayFactory<BookingCreatedMessage> _kafkaConsumerGateway;
    private readonly EventsKafkaSettings _settings;

    public EventsConsumer(IKafkaConsumerGatewayFactory<BookingCreatedMessage> kafkaConsumerGateway, EventsKafkaSettings eventsKafkaSettings)
    {
        _kafkaConsumerGateway = kafkaConsumerGateway;
        _settings = eventsKafkaSettings;
    }

    public Task HandleBookingCreatedMessage(Func<BookingCreatedMessage, CancellationToken, Task> func, CancellationToken ct)
    {
        var gateway = CreateGateway((message, token) => func(message.Content, token));
        return gateway.Run(ct);
    }

    private IKafkaConsumerGateway<BookingCreatedMessage> CreateGateway(
        Func<IncomingMessage<BookingCreatedMessage>, CancellationToken, Task> messageHandler)
    {
        return _kafkaConsumerGateway.CreateGateway(
            config: new ConsumerConfig
            {
                BootstrapServers = _settings.BaseUrl,
                SaslUsername = _settings.Login,
                SaslPassword = _settings.Password
            },
            topic: KafkaTopics.BookingTopic,
            messageHandler: messageHandler);
    }
}
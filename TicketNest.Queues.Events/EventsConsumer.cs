using Confluent.Kafka;
using TicketNest.Application.Events.Services;
using TicketNest.Contracts.Kafka;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Kafka.Consumer;
using TicketNest.Queues.Events.Settings;

namespace TicketNest.Queues.Events;

internal sealed class EventsConsumer : IEventsConsumer
{
    private readonly IKafkaConsumerGatewayFactory _kafkaConsumerGatewayFactory;
    private readonly EventsKafkaSettings _settings;

    public EventsConsumer(IKafkaConsumerGatewayFactory kafkaConsumerGatewayFactory, EventsKafkaSettings eventsKafkaSettings)
    {
        _kafkaConsumerGatewayFactory = kafkaConsumerGatewayFactory;
        _settings = eventsKafkaSettings;
    }

    public async Task HandleBookingCreatedMessage(Func<BookingCreatedMessage, CancellationToken, Task> func, CancellationToken ct)
    {
        using var gateway = CreateGateway<BookingCreatedMessage>((message, token) => func(message.Content, token));
        await gateway.Run(ct);
    }

    public async Task HandleBookingCancelledMessage(Func<BookingCancelledMessage, CancellationToken, Task> func, CancellationToken ct)
    {
        using var gateway = CreateGateway<BookingCancelledMessage>((message, token) => func(message.Content, token));
        await gateway.Run(ct);
    }

    private IKafkaConsumerGateway<T> CreateGateway<T>(Func<IncomingMessage<T>, CancellationToken, Task> messageHandler) where T: class
    {
        return _kafkaConsumerGatewayFactory.CreateGateway(
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
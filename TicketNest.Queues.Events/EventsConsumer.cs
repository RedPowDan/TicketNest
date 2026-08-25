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
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BaseUrl,
            GroupId = "ticketnest-events-service"
        };

        ApplySasl(config, _settings.Login, _settings.Password);

        return _kafkaConsumerGatewayFactory.CreateGateway(
            config: config,
            topic: KafkaTopics.BookingTopic,
            messageHandler: messageHandler);
    }

    private static void ApplySasl(ClientConfig config, string? login, string? password)
    {
        if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
        {
            config.SecurityProtocol = SecurityProtocol.SaslPlaintext;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = login;
            config.SaslPassword = password;
        }
    }
}
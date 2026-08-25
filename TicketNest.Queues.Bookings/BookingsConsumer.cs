using Confluent.Kafka;
using TicketNest.Application.Bookings.Services;
using TicketNest.Contracts.Kafka;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Kafka.Consumer;
using TicketNest.Queues.Bookings.Settings;

namespace TicketNest.Queues.Bookings;

internal sealed class BookingsConsumer : IBookingsConsumer
{
    private readonly IKafkaConsumerGatewayFactory _kafkaConsumerGatewayFactory;
    private readonly BookingKafkaSettings _settings;

    public BookingsConsumer(IKafkaConsumerGatewayFactory kafkaConsumerGatewayFactory, BookingKafkaSettings settings)
    {
        _kafkaConsumerGatewayFactory = kafkaConsumerGatewayFactory;
        _settings = settings;
    }

    public async Task HandleBookingApprovedMessage(Func<BookingApprovedMessage, CancellationToken, Task> func, CancellationToken ct)
    {
        using var gateway = CreateGateway((message, token) => func(message.Content, token));
        await gateway.Run(ct);
    }

    private IKafkaConsumerGateway<BookingApprovedMessage> CreateGateway(
        Func<IncomingMessage<BookingApprovedMessage>, CancellationToken, Task> messageHandler)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BaseUrl,
            GroupId = "ticketnest-bookings-service"
        };

        ApplySasl(config, _settings.Login, _settings.Password);

        return _kafkaConsumerGatewayFactory.CreateGateway(
            config: config,
            topic: KafkaTopics.EventTopic,
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
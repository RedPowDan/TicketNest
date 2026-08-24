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

    public Task HandleBookingApprovedMessage(Func<BookingApprovedMessage, CancellationToken, Task> func, CancellationToken ct)
    {
        var gateway = CreateGateway((message, token) => func(message.Content, token));
        return gateway.Run(ct);
    }

    private IKafkaConsumerGateway<BookingApprovedMessage> CreateGateway(
        Func<IncomingMessage<BookingApprovedMessage>, CancellationToken, Task> messageHandler)
    {
        return _kafkaConsumerGatewayFactory.CreateGateway(
            config: new ConsumerConfig
            {
                BootstrapServers = _settings.BaseUrl,
                SaslUsername = _settings.Login,
                SaslPassword = _settings.Password
            },
            topic: KafkaTopics.EventTopic,
            messageHandler: messageHandler);
    }

}
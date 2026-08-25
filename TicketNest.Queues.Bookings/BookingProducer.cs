using System.Text.Json;
using Confluent.Kafka;
using TicketNest.Contracts.Kafka;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Bookings.Services;
using TicketNest.Kafka.Producer;
using TicketNest.Queues.Bookings.Settings;

namespace TicketNest.Queues.Bookings;

internal sealed class BookingProducer : IBookingProducer
{
    private readonly IKafkaProducerGatewayFactory _kafkaProducerGatewayFactory;
    private readonly BookingKafkaSettings _settings;

    public BookingProducer(IKafkaProducerGatewayFactory kafkaProducerGatewayFactory, BookingKafkaSettings settings)
    {
        _kafkaProducerGatewayFactory = kafkaProducerGatewayFactory;
        _settings = settings;
    }

    public async Task BookingCreated(Guid bookingId, Guid eventId, CancellationToken ct)
    {
        var message = new BookingCreatedMessage(bookingId, eventId);
        var outgoingMessage = CreateMessage(bookingId, message);

        var gateway = CreateGateway();
        await gateway.Produce(
            outgoingMessage: outgoingMessage,
            topic: KafkaTopics.BookingTopic,
            ct);
    }

    public async Task BookingCanceled(Guid bookingId, Guid eventId, CancellationToken ct)
    {        
        var message = new BookingCancelledMessage(bookingId, eventId);
        var outgoingMessage = CreateMessage(bookingId, message);

        var gateway = CreateGateway();
        await gateway.Produce(
            outgoingMessage: outgoingMessage,
            topic: KafkaTopics.BookingTopic,
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
        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BaseUrl
        };

        ApplySasl(config, _settings.Login, _settings.Password);

        return _kafkaProducerGatewayFactory.Create(config);
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
using System.Text.Json;
using Confluent.Kafka;
using TicketNest.Contracts.Kafka;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Services;
using TicketNest.Kafka.Producer;
using TicketNest.Queues.Settings;

namespace TicketNest.Queues;

internal sealed class BookingProducer : IBookingProducer
{
    private readonly IKafkaProducerGatewayFactory _kafkaProducerGatewayFactory;
    private readonly BookingKafkaSettings _settings;

    public BookingProducer(IKafkaProducerGatewayFactory kafkaProducerGatewayFactory, BookingKafkaSettings settings)
    {
        _kafkaProducerGatewayFactory = kafkaProducerGatewayFactory;
        _settings = settings;
    }

    public async Task BookingCreated(Booking booking, CancellationToken ct)
    {
        var message = new BookingCreatedMessage(booking.Id, booking.EventId);
        var outgoingMessage = new Message<string, string>
        {
            Key = booking.Id.ToString(),
            Value = JsonSerializer.Serialize(message)
        };

        var gateway = CreateGateway();
        await gateway.Produce(
            outgoingMessage: outgoingMessage,
            topic: KafkaTopics.BookingTopic,
            ct);
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
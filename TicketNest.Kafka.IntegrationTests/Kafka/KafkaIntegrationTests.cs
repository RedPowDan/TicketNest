using System.Text.Json;
using Confluent.Kafka;
using Testcontainers.Kafka;
using TicketNest.Contracts.Kafka;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Kafka;
using Xunit;

namespace TicketNest.Kafka.IntegrationTests.Kafka;

public sealed class KafkaIntegrationTests : IAsyncLifetime
{
    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.6.0").Build();

    public async Task InitializeAsync()
    {
        await _kafka.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _kafka.DisposeAsync();
    }

    private string BootstrapServers => _kafka.GetBootstrapAddress();

    [Fact]
    public void EnsureTopicsCreated_Should_Create_Booking_And_Event_Topics()
    {
        KafkaTopicInitializer.EnsureTopicsCreated(BootstrapServers, null, null, KafkaTopics.BookingTopic, KafkaTopics.EventTopic);

        using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = BootstrapServers }).Build();
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(30));
        var topicNames = metadata.Topics.Select(t => t.Topic).ToHashSet();

        Assert.Contains(KafkaTopics.BookingTopic, topicNames);
        Assert.Contains(KafkaTopics.EventTopic, topicNames);
    }

    [Fact]
    public async Task Publish_BookingCreatedMessage_Then_Consume_Should_Receive_Same_Payload()
    {
        KafkaTopicInitializer.EnsureTopicsCreated(BootstrapServers, null, null, KafkaTopics.BookingTopic);

        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var message = new BookingCreatedMessage(bookingId, eventId);

        await ProduceAsync(message);

        var consumed = Consume(TimeSpan.FromSeconds(30));

        Assert.NotNull(consumed);
        Assert.Equal(bookingId, consumed!.BookingId);
        Assert.Equal(eventId, consumed.EventId);
    }

    private async Task ProduceAsync(BookingCreatedMessage message)
    {
        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
        }).Build();

        var envelope = new Message<string, string>
        {
            Key = message.BookingId.ToString(),
            Value = JsonSerializer.Serialize(message),
        };

        await producer.ProduceAsync(KafkaTopics.BookingTopic, envelope);
        producer.Flush(TimeSpan.FromSeconds(30));
    }

    private BookingCreatedMessage? Consume(TimeSpan timeout)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = "ticketnest-integration-" + Guid.NewGuid(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
        }).Build();

        consumer.Subscribe(KafkaTopics.BookingTopic);

        try
        {
            var result = consumer.Consume(timeout);
            if (result?.Message == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<BookingCreatedMessage>(result.Message.Value);
        }
        finally
        {
            consumer.Close();
        }
    }
}

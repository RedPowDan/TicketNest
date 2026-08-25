using System.Text.Json;

namespace TicketNest.Kafka.Consumer;

internal static class KafkaConsumerGatewaySettings
{
    public static readonly TimeSpan ConsumeTimeout = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan RetryDelayWhenEmptyTopic = TimeSpan.FromSeconds(10);

    public static readonly JsonSerializerOptions JsonSerializerOptions = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    });
}
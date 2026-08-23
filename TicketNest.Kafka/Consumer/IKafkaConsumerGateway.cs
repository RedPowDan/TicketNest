namespace TicketNest.Kafka.Consumer;

public interface IKafkaConsumerGateway<T> : IDisposable where T : class
{
    Task Run(CancellationToken ct);
}
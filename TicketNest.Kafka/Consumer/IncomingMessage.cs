namespace TicketNest.Kafka.Consumer;

public class IncomingMessage<T> where T : class
{
    public T Content { get; }

    public Metadata MessageMetadata { get; }

    public IncomingMessage(T content, int partition, long offset, DateTime createdUtc)
    {
        Content = content;
        MessageMetadata = new Metadata(partition, offset, createdUtc);
    }

    public sealed class Metadata
    {
        public long Offset { get; }

        public int Partition { get; }

        public DateTime CreatedUtc { get; }

        internal Metadata(int partition, long offset, DateTime createdUtc)
        {
            Partition = partition;
            Offset = offset;
            CreatedUtc = createdUtc;
        }
    }
}
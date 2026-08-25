using TicketNest.Shared.Guard;

namespace TicketNest.Shared;

public class QueueMessage<T> where T : class
{
    public string QueueName { get; }

    public Guid MessageId { get; }

    public T Data { get; }

    private QueueMessage(string queueName, Guid messageId, T data)
    {
        Ensure.NotNullOrEmpty(queueName, nameof(queueName));
        Ensure.NotDefault(messageId, nameof(messageId));
        Ensure.NotNull(data, nameof(data));

        QueueName = queueName;
        MessageId = messageId;
        Data = data;
    }

    public static QueueMessage<T> Create(string queueName, T data)
    {
        return new QueueMessage<T>(queueName, Guid.CreateVersion7(), data);
    }

    public static QueueMessage<T> LoadFromStorage(string queueName, Guid messageId, T data)
    {
        return new QueueMessage<T>(queueName, messageId, data);
    }
}
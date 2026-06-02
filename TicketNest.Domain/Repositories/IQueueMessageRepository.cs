using TicketNest.Domain.Models.Queue;

namespace TicketNest.Domain.Repositories;

public interface IQueueMessageRepository
{
    public Task Create<T>(QueueMessage<T> message, CancellationToken ct = default) where T : class;

    public Task<QueueMessage<T>?> Get<T>(string queueName, CancellationToken ct = default) where T : class;

    public Task<IReadOnlyCollection<QueueMessage<T>>> GetAll<T>(string queueName, CancellationToken ct = default) where T : class;

    public Task Commit(Guid messageId, CancellationToken ct = default);
}
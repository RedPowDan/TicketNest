using System.Collections.Concurrent;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Repositories;

namespace TicketNest.DataAccess.Queue.Implementations;

internal sealed class MemoryQueueMessageRepository : IQueueMessageRepository
{
    private static readonly ConcurrentDictionary<string, ConcurrentQueue<object>> Queues = new();

    public Task Create<T>(QueueMessage<T> message, CancellationToken ct = default) where T : class
    {
        var queue = Queues.GetOrAdd(message.QueueName, _ => new ConcurrentQueue<object>());
        queue.Enqueue(message);
        return Task.CompletedTask;
    }

    public Task<QueueMessage<T>?> Get<T>(string queueName, CancellationToken ct = default) where T : class
    {
        if (!Queues.TryGetValue(queueName, out var queue))
        {
            return Task.FromResult<QueueMessage<T>?>(null);
        }
        
        // Пытаемся достать сообщение из очереди
        if (queue.TryDequeue(out var rawMessage))
        {
            if (rawMessage is QueueMessage<T> typedMessage)
            {
                return Task.FromResult<QueueMessage<T>?>(typedMessage);
            }
        }
        
        return Task.FromResult<QueueMessage<T>?>(null);
    }

    public Task<IReadOnlyCollection<QueueMessage<T>>> GetAll<T>(string queueName, CancellationToken ct = default) where T : class
    {
        if (!Queues.TryGetValue(queueName, out var queue))
        {
            return Task.FromResult<IReadOnlyCollection<QueueMessage<T>>>([]);
        }

        var result = new List<QueueMessage<T>>();
        foreach (var message in TryDequeueAll(queue))
        {
            if (message is QueueMessage<T> typedMessage)
            {
                result.Add(typedMessage);
            }
        }

        return Task.FromResult<IReadOnlyCollection<QueueMessage<T>>>(result);
    }

    public Task Commit(Guid messageId, CancellationToken ct = default)
    {
        // При таком подходе Commit не нужен, так как сообщение удаляется при Get
        // так сделано, дабы не усложнять инфраструктурный код, который на проде всеравно не используется
        // Но если будет кафка, то мы будем помечать сообщение прочитанным (At-Least-Once)
        return Task.CompletedTask;
    }

    private static IEnumerable<object> TryDequeueAll(ConcurrentQueue<object> queue)
    {
        while (true)
        {
            if (!queue.TryDequeue(out var message))
            {
                break;
            }

            yield return message;
        }
    }
}
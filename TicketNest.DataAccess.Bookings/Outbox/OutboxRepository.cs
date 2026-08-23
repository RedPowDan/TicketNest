using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Bookings.DbContext;
using TicketNest.DataAccess.Bookings.Outbox.Mappers;
using TicketNest.DataAccess.Bookings.Outbox.PersistenceEvents;
using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;
using TicketNest.Domain.Bookings.Outbox;
using TicketNest.Domain.Bookings.Repositories;

namespace TicketNest.DataAccess.Bookings.Outbox;

internal sealed class OutboxRepository(BookingsDbContext dbContext) : IOutboxRepository
{
    private const int MaxRetryCount = 5;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public void Add(IBookingEvent domainEvent, CancellationToken ct = default)
    {
        Ensure.NotNull(domainEvent);

        var persistenceEvent = OutboxEventMapper.ToPersistence(domainEvent);
        var content = JsonSerializer.Serialize(persistenceEvent, persistenceEvent.GetType(), JsonOptions);

        dbContext.OutboxMessages.Add(new TicketNest.DataAccess.Bookings.Models.OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = persistenceEvent.GetType().AssemblyQualifiedName!,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            Status = Models.OutboxMessage.OutboxStatus.Pending,
            RetryCount = 0,
        });
    }

    public async Task<OutboxMessage[]> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        var entities = await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(x => x.Status == Models.OutboxMessage.OutboxStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken: ct);

        return entities.Select(ToDomain).ToArray();
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        var message = await dbContext.OutboxMessages.FindAsync([id], cancellationToken: ct);
        if (message is null)
        {
            return;
        }

        message.ProcessedAt = DateTime.UtcNow;
        message.Status = Models.OutboxMessage.OutboxStatus.Processed;

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        var message = await dbContext.OutboxMessages.FindAsync([id], cancellationToken: ct);
        if (message is null)
        {
            return;
        }

        message.RetryCount++;
        message.Error = error;

        message.Status = message.RetryCount >= MaxRetryCount
            ? Models.OutboxMessage.OutboxStatus.Failed
            : Models.OutboxMessage.OutboxStatus.Pending;

        await dbContext.SaveChangesAsync(ct);
    }

    private static OutboxMessage ToDomain(TicketNest.DataAccess.Bookings.Models.OutboxMessage entity)
    {
        var persistenceType = Type.GetType(entity.Type)
            ?? throw new InvalidOperationException($"Тип события не найден: {entity.Type}");

        var persistenceEvent = JsonSerializer.Deserialize(entity.Content, persistenceType, JsonOptions) as BookingPersistenceEvent
            ?? throw new InvalidOperationException($"Не удалось десериализовать событие {entity.Type}");

        var domainEvent = OutboxEventMapper.ToDomain(persistenceEvent);

        return new OutboxMessage(entity.Id, domainEvent);
    }
}

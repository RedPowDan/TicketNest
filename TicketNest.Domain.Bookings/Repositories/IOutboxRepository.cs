using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;
using TicketNest.Domain.Bookings.Outbox;

namespace TicketNest.Domain.Bookings.Repositories;

public interface IOutboxRepository
{
    /// <summary>Добавляет доменное событие в Outbox в рамках текущей транзакции DbContext.</summary>
    void Add(IBookingEvent domainEvent, CancellationToken ct = default);

    /// <summary>Возвращает необработанные сообщения в виде доменных <see cref="OutboxMessage"/> (Id + готовое событие).</summary>
    Task<OutboxMessage[]> GetPendingAsync(int batchSize, CancellationToken ct = default);

    Task MarkProcessedAsync(Guid id, CancellationToken ct = default);

    Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);
}

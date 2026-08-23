using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.Domain.Bookings.Outbox;

/// <summary>
/// Контракт обработчика доменного события из Outbox (уровень домена).
/// На один конкретный тип события можно зарегистрировать несколько обработчиков.
/// Сами реализации живут в слое Application и резолвятся через DI по закрытому generic-типу.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IBookingEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}

using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.Domain.Bookings.Outbox;

/// <summary>
/// Доменное сообщение Outbox: идентификатор сообщения и готовый объект доменного события.
/// Не содержит деталей персистентности (JSON, типы persistence-моделей и т.п.).
/// </summary>
public sealed record OutboxMessage(Guid Id, IBookingEvent Event);

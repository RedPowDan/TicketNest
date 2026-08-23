using TicketNest.Domain.Bookings.Models.Bookings.DomainEvents;

namespace TicketNest.DataAccess.Bookings.Outbox.PersistenceEvents;

/// <summary>
/// Персистанс-модель доменного события <see cref="BookingRejected"/>.
/// </summary>
internal sealed class BookingRejectedPersistenceEvent : BookingPersistenceEvent;

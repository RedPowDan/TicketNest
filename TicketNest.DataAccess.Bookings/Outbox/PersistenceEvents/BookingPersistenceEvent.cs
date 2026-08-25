namespace TicketNest.DataAccess.Bookings.Outbox.PersistenceEvents;

/// <summary>
/// Базовая персистанс-модель outbox-события бронирования.
/// Содержит только данные, необходимые для хранения и последующей реконструкции доменного события.
/// </summary>
internal abstract class BookingPersistenceEvent
{
    public Guid BookingId { get; set; }

    public Guid EventId { get; set; }
}

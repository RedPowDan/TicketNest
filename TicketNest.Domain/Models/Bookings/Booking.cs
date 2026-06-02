namespace TicketNest.Domain.Models.Bookings;

/// <summary>
/// Модель бронирования
/// </summary>
public class Booking
{
    public Guid Id { get; }

    public Guid EventId { get; }

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? ProcessedAt { get; private set; }

    private Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        Id = id;
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    public static Booking LoadFromStorage(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        return new Booking(id, eventId, status, createdAt, processedAt);
    }

    internal static Booking Create(Guid eventId, DateTime createdAt)
    {
        Ensure.That(createdAt.Kind == DateTimeKind.Utc, "CreatedAt должен иметь временную зону UTC");

        return new Booking(id: Guid.CreateVersion7(), eventId: eventId, status: BookingStatus.Pending, createdAt: createdAt, processedAt: null);
    }
    
    internal void Confirm(DateTime processedAt)
    {
        Ensure.That(processedAt.Kind == DateTimeKind.Utc, "CreatedAt должен иметь временную зону UTC");
        Ensure.That(CreatedAt < processedAt, $"{nameof(processedAt)}={processedAt} не может быть меньше чем {nameof(CreatedAt)}={CreatedAt}");

        Status = BookingStatus.Confirmed;
        ProcessedAt = processedAt;
    }

    public void Reject(DateTime processedAt)
    {
        Ensure.That(processedAt.Kind == DateTimeKind.Utc, "CreatedAt должен иметь временную зону UTC");
        Ensure.That(CreatedAt < processedAt, $"{nameof(processedAt)}={processedAt} не может быть меньше чем {nameof(CreatedAt)}={CreatedAt}");

        Status = BookingStatus.Rejected;
        ProcessedAt = processedAt;
    }
}
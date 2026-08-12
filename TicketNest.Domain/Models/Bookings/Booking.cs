using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Users;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Models.Bookings;

/// <summary>
/// Модель бронирования
/// </summary>
public class Booking
{
    public Guid Id { get; }

    public Guid EventId { get; }

    public Guid UserId { get; }

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? ProcessedAt { get; private set; }

    private Booking(Guid id, Guid eventId, Guid userId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    public static Booking LoadFromStorage(Guid id, Guid eventId, Guid userId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        return new Booking(id, eventId, userId, status, createdAt, processedAt);
    }

    internal static Booking Create(Guid eventId, Guid userId, DateTime createdAt)
    {
        Ensure.That(createdAt.Kind == DateTimeKind.Utc, "CreatedAt должен иметь временную зону UTC");

        return new Booking(id: Guid.CreateVersion7(), eventId: eventId, userId: userId, status: BookingStatus.Pending, createdAt: createdAt, processedAt: null);
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

    public void Cancel(DateTime processedAt)
    {
        Ensure.That(processedAt.Kind == DateTimeKind.Utc, "CreatedAt должен иметь временную зону UTC");
        Ensure.That(CreatedAt < processedAt, $"{nameof(processedAt)}={processedAt} не может быть меньше чем {nameof(CreatedAt)}={CreatedAt}");

        Status = BookingStatus.Canceled;
        ProcessedAt = processedAt;
    }

    /// <summary>
    /// Проверяет, может ли указанный пользователь отменить бронь.
    /// Свою бронь может отменить любой пользователь, чужую — только администратор.
    /// Бронь, находящаяся в статусе <see cref="BookingStatus.Canceled"/> или
    /// <see cref="BookingStatus.Rejected"/>, отменить нельзя.
    /// </summary>
    public UnitResult<Error> CanCancel(User user)
    {
        Ensure.NotNull(user, nameof(user));

        if (user.Role != UserRole.Admin && user.Id != UserId)
        {
            return UnitResult<Error>.FromFailure(new Error(ErrorCode.Forbidden, "Недостаточно прав для отмены чужой брони"));
        }

        if (Status == BookingStatus.Canceled)
        {
            return UnitResult<Error>.FromFailure(new Error(ErrorCode.BadRequest, "Бронь уже отменена"));
        }

        if (Status == BookingStatus.Rejected)
        {
            return UnitResult<Error>.FromFailure(new Error(ErrorCode.BadRequest, "Нельзя отменить отклонённую бронь"));
        }

        return UnitResult<Error>.FromSuccess();
    }
}
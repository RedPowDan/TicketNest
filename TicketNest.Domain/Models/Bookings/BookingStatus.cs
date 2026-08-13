namespace TicketNest.Domain.Models.Bookings;

/// <summary>
/// Статус бронирования
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// Бронь создана, ожидает обработки
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Бронь подтверждена
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Бронь отклонена
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Бронь отменена пользователем или администратором
    /// </summary>
    Canceled = 3
}
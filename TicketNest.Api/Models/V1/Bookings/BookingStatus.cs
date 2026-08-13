namespace TicketNest.Api.Models.V1.Bookings;

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
    /// Бронь отменена
    /// </summary>
    Canceled = 3
}
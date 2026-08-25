namespace TicketNest.Domain.Bookings.Constants;

/// <summary>
/// Статус коды для ошибочных операций
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// В ходе операции сущность не найдена.
    /// </summary>
    /// <remarks>
    /// Например: обновление/изменение агрегата
    /// </remarks>
    NotFound = 1,
    
    /// <summary>
    /// Плохой запрос на выполнение действия.
    /// </summary>
    /// <remarks>
    /// Например: отсутствие обязательного поля
    /// </remarks>
    BadRequest = 2,

    /// <summary>
    /// Конфликтная ситуация
    /// </summary>
    /// <remarks>
    /// Например, когда два пользователя одновременно пытаются забронировать последнее место
    /// </remarks>
    Conflict = 3,

    /// <summary>
    /// Пользователь не аутентифицирован или не может быть определён из контекста запроса.
    /// </summary>
    Unauthorized = 4,

    /// <summary>
    /// Доступ запрещён: недостаточно прав для выполнения операции.
    /// </summary>
    Forbidden = 5,
}
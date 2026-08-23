namespace TicketNest.Domain.Bookings.Models.Users;

/// <summary>
/// Роль пользователя.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Обычный пользователь.
    /// </summary>
    User = 0,

    /// <summary>
    /// Администратор.
    /// </summary>
    Admin = 1,
}
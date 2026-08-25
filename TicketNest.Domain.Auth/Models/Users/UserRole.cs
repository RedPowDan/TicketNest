namespace TicketNest.Domain.Auth.Models.Users;

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
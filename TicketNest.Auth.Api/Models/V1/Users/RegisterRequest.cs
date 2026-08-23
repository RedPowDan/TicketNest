using System.ComponentModel.DataAnnotations;
using TicketNest.Domain.Auth.Models.Users;

namespace TicketNest.Auth.Api.Models.V1.Users;

public class RegisterRequest
{
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    [Required] public string Login { get; init; } = null!;

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    [Required] public string Password { get; init; } = null!;

    /// <summary>
    /// Роль пользователя. По умолчанию — обычный пользователь.
    /// </summary>
    public UserRole? Role { get; init; }
}

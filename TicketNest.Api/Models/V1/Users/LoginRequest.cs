using System.ComponentModel.DataAnnotations;

namespace TicketNest.Api.Models.V1.Users;

public class LoginRequest
{
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    [Required] public string Login { get; init; } = null!;

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    [Required] public string Password { get; init; } = null!;
}

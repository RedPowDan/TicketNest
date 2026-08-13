using TicketNest.Domain.Models.Users;
using TicketNest.Domain.Services.Auth;

namespace TicketNest.Api.Services;

/// <summary>
/// Предоставляет данные текущего аутентифицированного пользователя из контекста запроса.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Данные текущего пользователя либо <c>null</c>, если запрос не аутентифицирован.
    /// </summary>
    TokenUser? User { get; }

    TokenUser GetUser();

    UserRole GetUserRole();
}

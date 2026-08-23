using System.Security.Claims;

namespace TicketNest.Domain.Auth.Services.Auth;

/// <summary>
/// Извлекает данные пользователя (<see cref="TokenUser"/>) из проверенного
/// субъекта (<see cref="ClaimsPrincipal"/>), полученного после аутентификации по JWT.
/// </summary>
public interface IJwtTokenReader
{
    /// <summary>
    /// Читает данные пользователя из claims субъекта.
    /// </summary>
    /// <param name="principal">Субъект, содержащий claims пользователя.</param>
    /// <returns>
    /// Данные пользователя либо <c>null</c>, если субъект не аутентифицирован
    /// или в нём отсутствуют обязательные claims.
    /// </returns>
    TokenUser? Read(ClaimsPrincipal principal);
}

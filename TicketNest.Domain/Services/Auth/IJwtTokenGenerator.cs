using TicketNest.Shared;

namespace TicketNest.Domain.Services.Auth;

/// <summary>
/// Формирует подписанный JWT-токен по данным пользователя.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Генерирует подписанный JWT-токен.
    /// </summary>
    /// <param name="user">Данные пользователя, которые попадут в claims токена.</param>
    /// <returns>Строковое представление JWT-токена.</returns>
    string GenerateToken(TokenUser user);
}

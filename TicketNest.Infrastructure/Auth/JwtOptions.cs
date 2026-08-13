namespace TicketNest.Infrastructure.Auth;

/// <summary>
/// Настройки, используемые для формирования JWT-токена.
/// </summary>
internal sealed class JwtOptions
{
    /// <summary>
    /// Секция конфигурации в appsettings.json.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Секретный ключ для подписи токена.
    /// </summary>
    public required string Secret { get; init; }

    /// <summary>
    /// Издатель токена (issuer).
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Аудитория токена (audience).
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    /// Время жизни токена в минутах.
    /// </summary>
    public required int LifetimeMinutes { get; init; }
}

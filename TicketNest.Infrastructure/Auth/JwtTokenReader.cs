using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TicketNest.Domain.Services.Auth;
using TicketNest.Shared.Guard;

namespace TicketNest.Infrastructure.Auth;

/// <summary>
/// Расшифровывает JWT-токен, извлекая данные пользователя из его claims.
/// </summary>
internal sealed class JwtTokenReader : IJwtTokenReader
{
    /// <inheritdoc />
    public TokenUser? Read(ClaimsPrincipal principal)
    {
        Ensure.NotNull(principal);

        if (principal.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
        var loginClaim = principal.FindFirst(ClaimTypes.Name);
        var roleClaim = principal.FindFirst(ClaimTypes.Role);

        if (idClaim is null || loginClaim is null || roleClaim is null)
        {
            return null;
        }

        if (!Guid.TryParse(idClaim.Value, out var id))
        {
            return null;
        }

        return TokenUser.Create(id, loginClaim.Value, roleClaim.Value);
    }
}

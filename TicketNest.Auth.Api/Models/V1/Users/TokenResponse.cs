using System.ComponentModel.DataAnnotations;

namespace TicketNest.Auth.Api.Models.V1.Users;

public class TokenResponse(string token)
{
    /// <summary>
    /// JWT-токен доступа.
    /// </summary>
    [Required] public string Token { get; init; } = token;
}

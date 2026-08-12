using TicketNest.Api.Exceptions;
using TicketNest.Api.Mappers;
using TicketNest.Domain.Models.Users;
using TicketNest.Domain.Services.Auth;

namespace TicketNest.Api.Services;

/// <summary>
/// Извлекает данные текущего пользователя из <see cref="HttpContext.User"/>,
/// расшифровывая JWT-токен через <see cref="IJwtTokenReader"/>.
/// </summary>
internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtTokenReader _jwtTokenReader;

    public CurrentUser(IHttpContextAccessor httpContextAccessor, IJwtTokenReader jwtTokenReader)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtTokenReader = jwtTokenReader;
    }

    /// <inheritdoc />
    public TokenUser? User
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return principal is null ? null : _jwtTokenReader.Read(principal);
        }
    }

    /// <inheritdoc />
    public TokenUser GetUser()
    {
        if (User is null)
        {
            throw new UnauthorizedException("User is null");
        }

        return User;
    }

    /// <inheritdoc />
    public UserRole GetUserRole()
    {
        return UserRoleMapper.Map(GetUser().Role);
    }
}

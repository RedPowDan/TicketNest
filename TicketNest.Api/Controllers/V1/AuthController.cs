using Microsoft.AspNetCore.Mvc;
using TicketNest.Api.Exceptions;
using TicketNest.Api.Models.V1;
using TicketNest.Api.Models.V1.Users;
using TicketNest.Application.Services.Users;

namespace TicketNest.Api.Controllers.V1;

[ApiController]
[Route("auth")]
[ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status500InternalServerError)]
public class AuthController(IUserService userService) : BaseApiController
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultModel<EmptyResultModel>>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await userService.Register(request.Login, request.Password, request.Role, ct);
        if (result.IsFailure)
        {
            ExceptionFactory.ThrowApiException(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Вход пользователя по логину и паролю. Возвращает JWT-токен.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ResultModel<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResultModel<TokenResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await userService.Login(request.Login, request.Password, ct);
        if (result.IsFailure)
        {
            ExceptionFactory.ThrowApiException(result.Error);
        }

        return Success(new TokenResponse(result.Value));
    }
}

using Microsoft.AspNetCore.Mvc;
using TicketNest.Auth.Api.Models.V1;

namespace TicketNest.Auth.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ResultModel<T>> Success<T>(T result) where T : class
    {
        return Ok(ResultModel<T>.FromSuccess(result));
    }
}
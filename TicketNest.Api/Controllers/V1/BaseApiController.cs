using Microsoft.AspNetCore.Mvc;
using TicketNest.Api.Models.V1;
using TicketNest.Api.Models.V1.Errors;

namespace TicketNest.Api.Controllers.V1;

public abstract class BaseApiController : ControllerBase
{
    private static readonly EmptyResultModel EmptyResultModel = new();
    
    protected ActionResult<ResultModel<T>> Created<T>(T result) where T : class
    {
        return base.Created(uri: (string?) null, ResultModel<T>.FromSuccess(result));
    }

    protected ActionResult<ResultModel<T>> Success<T>(T result) where T : class
    {
        return Ok(ResultModel<T>.FromSuccess(result));
    }

    protected ActionResult<ResultModel<EmptyResultModel>> Success()
    {
        return Ok(ResultModel<EmptyResultModel>.FromSuccess(EmptyResultModel));
    }

    protected ActionResult<ResultModel<T>> Failure<T>(string message, int statusCode = 400) where T : class
    {
        return StatusCode(statusCode, ResultModel<T>.FromFailure(new ErrorModel { Message = message }));
    }

    protected ActionResult<ResultModel<T>> NotFound<T>(string message = "Resource not found") where T : class
    {
        return NotFound(ResultModel<T>.FromFailure(new ErrorModel { Message = message }));
    }

    protected ActionResult<ResultModel<T>> BadRequest<T>(string message) where T : class
    {
        return BadRequest(ResultModel<T>.FromFailure(new ErrorModel { Message = message }));
    }
}
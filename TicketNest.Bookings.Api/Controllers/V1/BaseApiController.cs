using Microsoft.AspNetCore.Mvc;
using TicketNest.Bookings.Api.Models.V1;

namespace TicketNest.Bookings.Api.Controllers.V1;

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

    protected ActionResult<ResultModel<T>> Accepted<T>(string uri, T result) where T : class
    {
        return base.Accepted(uri, ResultModel<T>.FromSuccess(result));
    }
}
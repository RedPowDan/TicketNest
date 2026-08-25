using System.Net;
using TicketNest.Events.Api.Exceptions;
using TicketNest.Events.Api.Models.V1.Errors;

namespace TicketNest.Events.Api.Infrastructure;

internal static class ErrorModelFactory
{
    public static ErrorModel Create(Exception ex) =>
        ex switch
        {
            ArgumentException argumentException => new ErrorModel(
                errorCode: ErrorCode.BadRequest,
                message: argumentException.Message,
                detail: argumentException.ParamName,
                httpStatusCode: HttpStatusCode.BadRequest),
            ApiException apiException => new ErrorModel(
                errorCode: apiException.ErrorCode,
                message: apiException.Message,
                detail: apiException.Detail,
                httpStatusCode: apiException.HttpStatusCode),
            _ => new ErrorModel(
                errorCode: ErrorCode.InternalServerError,
                message: "Упс, что-то пошло не так, попробуйте позже",
                detail: null,
                httpStatusCode: HttpStatusCode.InternalServerError)
        };
}
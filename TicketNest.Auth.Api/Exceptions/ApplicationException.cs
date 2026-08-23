using System.Net;
using TicketNest.Auth.Api.Models.V1.Errors;

namespace TicketNest.Auth.Api.Exceptions;

public abstract class ApiException : Exception
{
    public new string Message { get; }

    public ErrorCode ErrorCode { get; }

    public string? Detail { get; }

    public HttpStatusCode HttpStatusCode { get; }

    protected ApiException(string message, ErrorCode errorCode, string? detail, HttpStatusCode httpStatusCode) : base(message)
    {
        Message = message;
        ErrorCode = errorCode;
        Detail = detail;
        HttpStatusCode = httpStatusCode;
    }
}
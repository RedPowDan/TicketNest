using TicketNest.Domain.Constants;

namespace TicketNest.Domain.Models;

public sealed class Error
{
    public ErrorCode StatusCode { get; }
    public string Message { get; }

    public Error(ErrorCode statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }
}
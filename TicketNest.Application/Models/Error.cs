using TicketNest.Application.Constants;
using TicketNest.Shared.Guard;

namespace TicketNest.Application.Models;

public class Error
{
    public ErrorStatusCode StatusCode { get; }

    public string Message { get; }

    public Error(ErrorStatusCode statusCode, string message)
    {
        Ensure.NotNullOrEmpty(message, nameof(message));

        StatusCode = statusCode;
        Message = message;
    }
}
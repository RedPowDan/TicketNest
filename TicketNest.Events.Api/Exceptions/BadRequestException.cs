using System.Net;
using TicketNest.Events.Api.Models.V1.Errors;

namespace TicketNest.Events.Api.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message, string? detail = null) : base(
        message,
        ErrorCode.BadRequest,
        detail,
        HttpStatusCode.BadRequest)
    {
    }
}
using System.Net;
using TicketNest.Events.Api.Models.V1.Errors;

namespace TicketNest.Events.Api.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string message, string? detail = null)
        : base(
            message,
            ErrorCode.NotFound,
            detail,
            HttpStatusCode.NotFound)
    {
    }
}
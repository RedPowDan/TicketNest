using System.Net;
using TicketNest.Api.Models.V1.Errors;

namespace TicketNest.Api.Exceptions;

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
using System.Net;
using TicketNest.Auth.Api.Models.V1.Errors;

namespace TicketNest.Auth.Api.Exceptions;

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
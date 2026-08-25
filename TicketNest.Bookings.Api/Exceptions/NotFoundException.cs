using System.Net;
using TicketNest.Bookings.Api.Models.V1.Errors;

namespace TicketNest.Bookings.Api.Exceptions;

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
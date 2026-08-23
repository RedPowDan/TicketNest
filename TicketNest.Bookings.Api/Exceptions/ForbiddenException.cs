using System.Net;
using TicketNest.Bookings.Api.Models.V1.Errors;

namespace TicketNest.Bookings.Api.Exceptions;

public sealed class ForbiddenException(string message) : ApiException(
    message,
    ErrorCode.Forbidden,
    detail: null,
    HttpStatusCode.Forbidden)
{
}

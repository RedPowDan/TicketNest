using System.Net;
using TicketNest.Bookings.Api.Models.V1.Errors;

namespace TicketNest.Bookings.Api.Exceptions;

public sealed class UnauthorizedException(string message) : ApiException(
    message,
    ErrorCode.Unauthorized,
    detail: null,
    HttpStatusCode.Unauthorized)
{
}

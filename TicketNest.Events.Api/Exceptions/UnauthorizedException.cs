using System.Net;
using TicketNest.Events.Api.Models.V1.Errors;

namespace TicketNest.Events.Api.Exceptions;

public sealed class UnauthorizedException(string message) : ApiException(
    message,
    ErrorCode.Unauthorized,
    detail: null,
    HttpStatusCode.Unauthorized)
{
}

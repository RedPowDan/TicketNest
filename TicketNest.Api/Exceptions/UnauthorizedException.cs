using System.Net;
using TicketNest.Api.Models.V1.Errors;

namespace TicketNest.Api.Exceptions;

public sealed class UnauthorizedException(string message) : ApiException(
    message,
    ErrorCode.Unauthorized,
    detail: null,
    HttpStatusCode.Unauthorized)
{
}

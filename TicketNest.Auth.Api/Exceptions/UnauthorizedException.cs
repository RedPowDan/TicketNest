using System.Net;
using TicketNest.Auth.Api.Models.V1.Errors;

namespace TicketNest.Auth.Api.Exceptions;

public sealed class UnauthorizedException(string message) : ApiException(
    message,
    ErrorCode.Unauthorized,
    detail: null,
    HttpStatusCode.Unauthorized)
{
}

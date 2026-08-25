using System.Net;
using TicketNest.Auth.Api.Models.V1.Errors;

namespace TicketNest.Auth.Api.Exceptions;

public sealed class ForbiddenException(string message) : ApiException(
    message,
    ErrorCode.Forbidden,
    detail: null,
    HttpStatusCode.Forbidden)
{
}

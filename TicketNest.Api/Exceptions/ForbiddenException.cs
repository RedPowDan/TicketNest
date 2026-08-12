using System.Net;
using TicketNest.Api.Models.V1.Errors;

namespace TicketNest.Api.Exceptions;

public sealed class ForbiddenException(string message) : ApiException(
    message,
    ErrorCode.Forbidden,
    detail: null,
    HttpStatusCode.Forbidden)
{
}

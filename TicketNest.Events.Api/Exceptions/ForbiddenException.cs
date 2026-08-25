using System.Net;
using TicketNest.Events.Api.Models.V1.Errors;

namespace TicketNest.Events.Api.Exceptions;

public sealed class ForbiddenException(string message) : ApiException(
    message,
    ErrorCode.Forbidden,
    detail: null,
    HttpStatusCode.Forbidden)
{
}

using System.Net;
using TicketNest.Auth.Api.Models.V1.Errors;

namespace TicketNest.Auth.Api.Exceptions;

public class ConflictRequestException : ApiException
{
    public ConflictRequestException(string message, string? detail = null) : base(message, ErrorCode.Conflict, detail, HttpStatusCode.Conflict)
    {
    }
}
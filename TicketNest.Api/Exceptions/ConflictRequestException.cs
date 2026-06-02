using System.Net;
using TicketNest.Api.Models.V1.Errors;

namespace TicketNest.Api.Exceptions;

public class ConflictRequestException : ApiException
{
    public ConflictRequestException(string message, string? detail = null) : base(message, ErrorCode.Conflict, detail, HttpStatusCode.Conflict)
    {
    }
}
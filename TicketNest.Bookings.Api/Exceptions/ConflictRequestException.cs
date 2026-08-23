using System.Net;
using TicketNest.Bookings.Api.Models.V1.Errors;

namespace TicketNest.Bookings.Api.Exceptions;

public class ConflictRequestException : ApiException
{
    public ConflictRequestException(string message, string? detail = null) : base(message, ErrorCode.Conflict, detail, HttpStatusCode.Conflict)
    {
    }
}
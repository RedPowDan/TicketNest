namespace TicketNest.Events.Api.Models.V1.Errors;

public enum ErrorCode
{
    InternalServerError = 0,
    BadRequest = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
}
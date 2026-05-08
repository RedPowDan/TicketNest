using TicketNest.Application.Constants;
using TicketNest.Application.Models;

namespace TicketNest.Api.Exceptions;

public static class ExceptionFactory
{
    public static void ThrowApiException(Error error)
    {
        throw error.StatusCode switch
        {
            ErrorStatusCode.NotFound => new NotFoundException(error.Message),
            ErrorStatusCode.BadRequest => new BadRequestException(error.Message),
            _ => new ArgumentOutOfRangeException()
        };
    }
}
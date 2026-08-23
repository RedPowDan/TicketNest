using TicketNest.Domain.Events.Models;
using DomainErrorCode = TicketNest.Domain.Events.Constants.ErrorCode;

namespace TicketNest.Events.Api.Exceptions;

public static class ExceptionFactory
{
    public static void ThrowApiException(Error error)
    {
        throw error.StatusCode switch
        {
            DomainErrorCode.NotFound => new NotFoundException(error.Message),
            DomainErrorCode.BadRequest => new BadRequestException(error.Message),
            DomainErrorCode.Conflict => new ConflictRequestException(error.Message),
            DomainErrorCode.Unauthorized => new UnauthorizedException(error.Message),
            DomainErrorCode.Forbidden => new ForbiddenException(error.Message),
            _ => new ArgumentOutOfRangeException()
        };
    }
}
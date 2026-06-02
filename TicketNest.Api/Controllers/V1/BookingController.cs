using Microsoft.AspNetCore.Mvc;
using TicketNest.Api.Exceptions;
using TicketNest.Api.Mappers.Bookings;
using TicketNest.Api.Models.V1;
using TicketNest.Api.Models.V1.Bookings;
using TicketNest.Application.Services.Bookings;

namespace TicketNest.Api.Controllers.V1;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status500InternalServerError)]
public class BookingController(IBookingService bookingService) : BaseApiController
{
    /// <summary>
    /// Получить бронирование по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultModel<BookingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultModel<BookingResponse>>> Get(Guid id, CancellationToken ct = default)
    {
        var bookingResult = await bookingService.Get(id, ct);
        if (bookingResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(bookingResult.Error);
        }

        return Success(BookingResponseMapper.Map(bookingResult.Value));
    }
}
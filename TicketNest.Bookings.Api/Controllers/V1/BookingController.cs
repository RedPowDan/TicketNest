using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketNest.Application.Bookings.Services.Bookings;
using TicketNest.Bookings.Api.Exceptions;
using TicketNest.Bookings.Api.Mappers.Bookings;
using TicketNest.Bookings.Api.Models.V1;
using TicketNest.Bookings.Api.Models.V1.Bookings;
using TicketNest.Bookings.Api.Services;

namespace TicketNest.Bookings.Api.Controllers.V1;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status500InternalServerError)]
public class BookingController(IBookingService bookingService, ICurrentUser currentUser) : BaseApiController
{
    /// <summary>
    /// Получить бронирование по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ResultModel<BookingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Отменить бронирование. Свою бронь может отменить любой пользователь,
    /// чужую — только администратор.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultModel<EmptyResultModel>>> Cancel(Guid id, CancellationToken ct)
    {
        var user = currentUser.GetUser();

        var cancelResult = await bookingService.Cancel(id, user.Id, currentUser.GetUserRole(), ct);
        if (cancelResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(cancelResult.Error);
        }

        return NoContent();
    }
    
    /// <summary>
    /// Создание бронирования на событие
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/book")]
    [ProducesResponseType(typeof(ResultModel<BookingResponse>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResultModel<BookingResponse>>> Book(Guid id, CancellationToken ct)
    {
        var user = currentUser.GetUser();
        var createResult = await bookingService.Create(id, user.Id, ct);
        if (createResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(createResult.Error);
        }

        var booking = createResult.Value;
        var locationUrl = Url.Action(nameof(BookingController.Get), "Booking", new { id = booking.Id }, Request.Scheme);
        return Accepted(locationUrl!, BookingResponseMapper.Map(booking));
    }
}
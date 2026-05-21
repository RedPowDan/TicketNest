using Microsoft.AspNetCore.Mvc;
using TicketNest.Api.Exceptions;
using TicketNest.Api.Mappers;
using TicketNest.Api.Mappers.Bookings;
using TicketNest.Api.Mappers.Events;
using TicketNest.Api.Models;
using TicketNest.Api.Models.V1;
using TicketNest.Api.Models.V1.Bookings;
using TicketNest.Api.Models.V1.Events;
using TicketNest.Application.Services.Bookings;
using TicketNest.Application.Services.Events;

namespace TicketNest.Api.Controllers.V1;

[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService, IBookingService bookingService) : BaseApiController
{
    /// <summary>
    /// Получить список всех событий
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResultModel<PaginatedResultModel<EventResponse>>>> Get(
        [FromQuery] EventsFilterModel filter,
        [FromQuery] PaginationRequestModel pagination,
        CancellationToken ct)
    {
        var paginatedResult = await eventService.GetAll(
            filter: EventsFilterMapper.Map(filter),
            paginationRequest: PaginationRequestMapper.Map(pagination),
            ct: ct);

        var paginatedModel = new PaginatedResultModel<EventResponse>()
        {
            Items = paginatedResult.Items.Select(EventResponseMapper.Map).ToArray(),
            TotalCount = paginatedResult.TotalCount,
            Count = paginatedResult.Count,
            CurrentPage = paginatedResult.CurrentPage,
        };

        return Success(paginatedModel);
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResultModel<EventResponse>>> Get(Guid id, CancellationToken ct)
    {
        var @event = await eventService.Get(id, ct);
        if (@event == null)
        {
            throw new NotFoundException("Не найдено событие");
        }

        return Success(EventResponseMapper.Map(@event));
    }

    /// <summary>
    /// Создать событие
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResultModel<EventResponse>>> Post([FromBody] EventRequest source, CancellationToken ct)
    {
        var createResult = await eventService.Create(
            source.Title,
            source.Description,
            source.StartAt,
            source.EndAt,
            ct);
        if (createResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(createResult.Error);
        }

        return Created(EventResponseMapper.Map(createResult.Value));
    }

    /// <summary>
    /// Изменить событие
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResultModel<EmptyResultModel>>> Put(Guid id, [FromBody] EventRequest source, CancellationToken ct)
    {
        var changeResult = await eventService.Change(
            id,
            source.Title,
            source.Description,
            source.StartAt,
            source.EndAt,
            ct);
        if (changeResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(changeResult.Error);
        }

        return Success();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ResultModel<EmptyResultModel>>> Delete(Guid id, CancellationToken ct)
    {
        var deleteResult = await eventService.Delete(id, ct);
        if (deleteResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(deleteResult.Error);
        }

        return Success();
    }

    /// <summary>
    /// Создание бронирования на событие
    /// </summary>
    [HttpPost("{id:guid}/book")]
    public async Task<ActionResult<ResultModel<BookingResponse>>> Book([FromBody] BookCreateRequest source, CancellationToken ct)
    {
        var createResult = await bookingService.Create(source.EventId, ct);
        if (createResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(createResult.Error);
        }

        var booking = createResult.Value;
        var locationUrl = Url.Action(nameof(BookingController.Get), "Booking", new { id = booking.Id }, Request.Scheme);
        return Accepted(locationUrl!, BookingResponseMapper.Map(booking));
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketNest.Application.Events.Services.Events;
using TicketNest.Events.Api.Exceptions;
using TicketNest.Events.Api.Mappers;
using TicketNest.Events.Api.Mappers.Events;
using TicketNest.Events.Api.Models;
using TicketNest.Events.Api.Models.V1;
using TicketNest.Events.Api.Models.V1.Events;

namespace TicketNest.Events.Api.Controllers.V1;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status500InternalServerError)]
public class EventsController(IEventService eventService) : BaseApiController
{
    /// <summary>
    /// Получить список всех событий
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultModel<PaginatedResultModel<EventResponse>>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(ResultModel<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status404NotFound)]
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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResultModel<EventResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ResultModel<EventResponse>>> Post([FromBody] EventRequest source, CancellationToken ct)
    {
        var createResult = await eventService.Create(
            source.Title,
            source.Description,
            source.StartAt,
            source.EndAt,
            source.TotalSeats,
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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status404NotFound)]
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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultModel<EmptyResultModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultModel<EmptyResultModel>>> Delete(Guid id, CancellationToken ct)
    {
        var deleteResult = await eventService.Delete(id, ct);
        if (deleteResult.IsFailure)
        {
            ExceptionFactory.ThrowApiException(deleteResult.Error);
        }

        return Success();
    }
}
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TicketNest.Api.Constants;
using TicketNest.Api.Mappers.Events;
using TicketNest.Api.Models.V1;
using TicketNest.Api.Models.V1.Events;
using TicketNest.Application.Constants;
using TicketNest.Application.Services.Events;

namespace TicketNest.Api.Controllers.V1;

[ApiController]
[ApiVersion(Versioning.V1)]
[Route("v{version:apiVersion}/[controller]")]
public class EventsController(IEventService eventService) : BaseApiController
{
    /// <summary>
    /// Получить список всех событий
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResultModel<EventResponse[]>>> Get(CancellationToken ct)
    {
        var events = await eventService.GetAll(ct);
        return Success(events.Select(EventResponseMapper.Map).ToArray());
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
            return NotFound<EventResponse>();
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
            return BadRequest<EventResponse>(createResult.Error.Message);
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
            return changeResult.Error.StatusCode == ErrorStatusCode.NotFound
                ? NotFound<EmptyResultModel>(changeResult.Error.Message)
                : BadRequest<EmptyResultModel>(changeResult.Error.Message);
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
            return deleteResult.Error.StatusCode == ErrorStatusCode.NotFound
                ? NotFound<EmptyResultModel>(deleteResult.Error.Message)
                : BadRequest<EmptyResultModel>(deleteResult.Error.Message);
        }

        return Success();
    }
}
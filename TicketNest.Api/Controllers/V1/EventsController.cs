using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TicketNest.Api.Constants;
using TicketNest.Api.Mappers.Events;
using TicketNest.Api.Models.V1.Errors;
using TicketNest.Api.Models.V1.Events;
using TicketNest.Application.Constants;
using TicketNest.Application.Services.Events;
using TicketNest.Domain.ValueObjects;
using EventId = TicketNest.Domain.ValueObjects.EventId;

namespace TicketNest.Api.Controllers.V1;

[ApiController]
[ApiVersion(Versioning.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController(IEventService eventService) : Controller
{
    /// <summary>
    /// Получить список всех событий
    /// </summary>
    [HttpGet("events")]
    public async Task<ActionResult<EventResponse[]>> Get(CancellationToken ct)
    {
        var events = await eventService.GetAll(ct);
        return Ok(events.Select(EventResponseMapper.Map).ToArray());
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    [HttpGet("events/{id:guid}")]
    public async Task<ActionResult<EventResponse>> Get(Guid id, CancellationToken ct)
    {
        var @event = await eventService.Get(EventId.From(id), ct);
        if (@event == null)
        {
            return NotFound();
        }

        return Ok(EventResponseMapper.Map(@event));
    }

    /// <summary>
    /// Создать событие
    /// </summary>
    [HttpPost("events")]
    public async Task<ActionResult<EventResponse>> Post([FromBody] EventRequest source, CancellationToken ct)
    {
        var createResult = await eventService.Create(
            EventTitle.From(source.Title),
            source.Description == null ? null : EventDescription.From(source.Description),
            source.StartAt,
            source.EndAt,
            ct);
        if (createResult.IsFailure)
        {
            return BadRequest(new ErrorModel { Message = createResult.Error.Message });
        }

        return Created(uri: (string?) null, value: EventResponseMapper.Map(createResult.Value));
    }

    /// <summary>
    /// Изменить событие
    /// </summary>
    [HttpPut("events/{id:guid}")]
    public async Task<ActionResult> Put(Guid id, [FromBody] EventRequest source, CancellationToken ct)
    {
        var changeResult = await eventService.Change(
            EventId.From(id),
            EventTitle.From(source.Title),
            source.Description == null ? null : EventDescription.From(source.Description),
            source.StartAt,
            source.EndAt,
            ct);
        if (changeResult.IsFailure)
        {
            return BadRequest(new ErrorModel { Message = changeResult.Error.Message });
        }

        return Ok();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    [HttpDelete("events/{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleteResult = await eventService.Delete(EventId.From(id), ct);
        if (deleteResult.IsFailure)
        {
            var error = new ErrorModel { Message = deleteResult.Error.Message };
            return deleteResult.Error.StatusCode == ErrorStatusCode.NotFound
                ? NotFound(error)
                : BadRequest(error);
        }

        return Ok();
    }
}
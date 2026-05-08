using Microsoft.AspNetCore.Mvc;
using TicketNest.Api.Exceptions;
using TicketNest.Api.Mappers.Events;
using TicketNest.Api.Models.V1;
using TicketNest.Api.Models.V1.Events;
using TicketNest.Application.Services.Events;

namespace TicketNest.Api.Controllers.V1;

[ApiController]
[Route("[controller]")]
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
}
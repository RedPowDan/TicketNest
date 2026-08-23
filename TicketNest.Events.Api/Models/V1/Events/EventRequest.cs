using System.ComponentModel.DataAnnotations;

namespace TicketNest.Events.Api.Models.V1.Events;

public class EventRequest
{
    /// <summary>
    /// Название события
    /// </summary>
    [Required] public string Title { get; init; } = null!;

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата начала события
    /// </summary>
    [Required] public DateTime StartAt { get; init; }

    /// <summary>
    /// Дата окончания события
    /// </summary>
    [Required] public DateTime EndAt { get; init; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    [Required] public int TotalSeats { get; init; }
}
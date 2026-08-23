namespace TicketNest.Events.Api.Models.V1.Events;

/// <summary>
/// Фильтр событий
/// </summary>
public sealed class EventsFilterModel
{
    /// <summary>
    /// Поиск по названию.
    /// </summary>
    /// <remarks>
    /// Регистронезависимый, частичное совпадение.
    /// </remarks>
    public string? Title { get; set; }

    /// <summary>
    /// События, которые начинаются не раньше указанной даты.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// События, которые заканчиваются не позже указанной даты.
    /// </summary>
    public DateTime? To { get; set; }
}
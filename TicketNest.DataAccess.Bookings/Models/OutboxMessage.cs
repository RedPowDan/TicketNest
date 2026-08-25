namespace TicketNest.DataAccess.Bookings.Models;

/// <summary>
/// Сообщение Outbox. Менять нельзя (согласно требованиям).
/// Хранит сериализованное persistence-событие и метаданные обработки.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty; // AssemblyQualifiedName persistence-события

    public string Content { get; set; } = string.Empty; // JSON сериализованное persistence-событие

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int RetryCount { get; set; }

    public string Error { get; set; } = string.Empty;

    public OutboxStatus Status { get; set; }

    public enum OutboxStatus
    {
        Pending,
        Processed,
        Failed
    }
}

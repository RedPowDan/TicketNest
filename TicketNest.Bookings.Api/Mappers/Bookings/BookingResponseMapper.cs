using TicketNest.Bookings.Api.Models.V1.Bookings;
using TicketNest.Domain.Bookings.Models.Bookings;

namespace TicketNest.Bookings.Api.Mappers.Bookings;

internal static class BookingResponseMapper
{
    public static BookingResponse Map(Booking source) =>
        new()
        {
            Id = source.Id,
            EventId = source.EventId,
            Status = BookingStatusMapper.Map(source.Status),
            CreatedAt = source.CreatedAt,
            ProcessedAt = source.ProcessedAt,
        };
}
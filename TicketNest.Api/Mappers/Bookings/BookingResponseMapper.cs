using TicketNest.Api.Models.V1.Bookings;
using TicketNest.Domain.Models.Bookings;

namespace TicketNest.Api.Mappers.Bookings;

internal static class BookingResponseMapper
{
    public static BookingResponse Map(Booking source) =>
        new()
        {
            Id = source.Id,
            EventId = source.EventId,
            Status = BookingStatusMapper.Map(source.Status),
        };
}
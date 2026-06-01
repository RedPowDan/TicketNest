using TicketNest.Api.Models.V1.Bookings;
using DomainBookingStatus = TicketNest.Domain.Models.Bookings.BookingStatus;

namespace TicketNest.Api.Mappers.Bookings;

internal static class BookingStatusMapper
{
    public static BookingStatus Map(DomainBookingStatus source) =>
        source switch
        {
            DomainBookingStatus.Pending => BookingStatus.Pending,
            DomainBookingStatus.Confirmed => BookingStatus.Confirmed,
            DomainBookingStatus.Rejected => BookingStatus.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
}
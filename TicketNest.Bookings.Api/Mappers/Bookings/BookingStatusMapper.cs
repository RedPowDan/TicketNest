using TicketNest.Bookings.Api.Models.V1.Bookings;
using DomainBookingStatus = TicketNest.Domain.Bookings.Models.Bookings.BookingStatus;

namespace TicketNest.Bookings.Api.Mappers.Bookings;

internal static class BookingStatusMapper
{
    public static BookingStatus Map(DomainBookingStatus source) =>
        source switch
        {
            DomainBookingStatus.Pending => BookingStatus.Pending,
            DomainBookingStatus.Confirmed => BookingStatus.Confirmed,
            DomainBookingStatus.Rejected => BookingStatus.Rejected,
            DomainBookingStatus.Canceled => BookingStatus.Canceled,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
}
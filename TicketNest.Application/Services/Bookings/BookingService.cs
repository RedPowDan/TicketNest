using TicketNest.Domain.Constants;
using TicketNest.Domain.Factories.Bookings;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Services.Bookings;

internal sealed class BookingService(IBookingFactory bookingFactory, IBookingRepository bookingRepository) : IBookingService
{
    public async Task<Result<Booking, Error>> Create(Guid eventId, CancellationToken ct = default)
    {
        var bookingCreateResult = await bookingFactory.Create(eventId, ct);
        if (bookingCreateResult.IsFailure)
        {
            return bookingCreateResult;
        }

        var booking = bookingCreateResult.Value;
        await bookingRepository.Save(booking, ct);

        return booking;
    }

    public async Task<Result<Booking, Error>> Get(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingRepository.Get(id, ct);
        if (booking == null)
        {
            return new Error(ErrorCode.NotFound, "Бронирование не найдено");
        }

        return booking;
    }
}
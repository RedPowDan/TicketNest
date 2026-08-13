using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Repositories;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Services.Bookings;

public class BookingFactory(IEventsRepository eventsRepository) : IBookingFactory
{
    public async Task<Result<Booking, Error>> Create(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var @event = await eventsRepository.Get(eventId, ct);
        if (@event is null)
        {
            return new Error(message: "Событие не найдено", statusCode: ErrorCode.NotFound);
        }

        return Booking.Create(eventId, userId, createdAt: DateTime.UtcNow);
    }
}
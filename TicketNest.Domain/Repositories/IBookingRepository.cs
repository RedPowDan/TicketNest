using TicketNest.Domain.Models.Bookings;

namespace TicketNest.Domain.Repositories;

public interface IBookingRepository
{
    Task Save(Booking booking, CancellationToken ct = default);

    ValueTask<Booking?> Get(Guid id, CancellationToken ct = default);
}
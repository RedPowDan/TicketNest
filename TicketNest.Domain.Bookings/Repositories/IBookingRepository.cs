using TicketNest.Domain.Bookings.Models.Bookings;

namespace TicketNest.Domain.Bookings.Repositories;

public interface IBookingRepository
{
    Task Save(Booking booking, CancellationToken ct = default);

    ValueTask<Booking?> Get(Guid id, CancellationToken ct = default);
    
    Task<Booking[]> GetBookingsByUserId(Guid userId, CancellationToken ct = default); 
}
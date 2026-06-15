using TicketNest.DataAccess.Events.DbContext;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Repositories;

namespace TicketNest.DataAccess.Events.Implementations;

internal sealed class BookingRepository(EventsDbContext dbContext) : IBookingRepository
{
    public async Task Save(Booking booking, CancellationToken ct = default)
    {
        Ensure.NotNull(booking, nameof(booking));

        var persistenceBooking = await dbContext
            .Bookings
            .FindAsync([booking.Id], cancellationToken: ct);
        if (persistenceBooking != null)
        {
            BookingMapper.Map(booking, persistenceBooking);
        }
        else
        {
            persistenceBooking = BookingMapper.ToPersistence(booking);
            dbContext.Bookings.Add(persistenceBooking);
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public async ValueTask<Booking?> Get(Guid id, CancellationToken ct = default)
    {
        var persistenceBooking = await dbContext
            .Bookings
            .FindAsync([id], cancellationToken: ct);

        return persistenceBooking == null
            ? null
            : BookingMapper.ToDomain(persistenceBooking);
    }
}
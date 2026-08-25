using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Bookings.DbContext;
using TicketNest.DataAccess.Bookings.Mappers;
using TicketNest.DataAccess.Bookings.Outbox;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Repositories;

namespace TicketNest.DataAccess.Bookings.Implementations;

internal sealed class BookingRepository(
    BookingsDbContext dbContext,
    IOutboxRepository outboxRepository) : IBookingRepository
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

        // Доменные события пишутся в Outbox той же транзакцией/тем же DbContext,
        // что и сама бронь — гарантирует атомарность (Outbox pattern).
        foreach (var domainEvent in booking.GetEvents())
        {
            outboxRepository.Add(domainEvent, ct);
        }

        await dbContext.SaveChangesAsync(ct);

        booking.Saved();
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

    /// <inheritdoc />
    public async Task<Booking[]> GetBookingsByUserId(Guid userId, CancellationToken ct = default)
    {
        var persistenceBookings = await dbContext
            .Bookings
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToArrayAsync(cancellationToken: ct);

        return persistenceBookings.Select(BookingMapper.ToDomain).ToArray();
    }
}
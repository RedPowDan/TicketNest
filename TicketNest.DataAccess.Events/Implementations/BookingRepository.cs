using System.Collections.Concurrent;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Repositories;

namespace TicketNest.DataAccess.Events.Implementations;

internal sealed class BookingRepository : IBookingRepository
{
    private static readonly ConcurrentDictionary<Guid, PersistenceBooking> Bookings = new();

    public Task Save(Booking booking, CancellationToken ct = default)
    {
        Ensure.NotNull(booking, nameof(booking));

        var persistenceBooking = BookingMapper.ToPersistence(booking);

        Bookings.AddOrUpdate(persistenceBooking.Id, persistenceBooking, (_, _) => persistenceBooking);

        return Task.CompletedTask;
    }

    public ValueTask<Booking?> Get(Guid id, CancellationToken ct = default)
    {
        Bookings.TryGetValue(id, out var persistenceBooking);

        return ValueTask.FromResult(persistenceBooking == null ? null : BookingMapper.ToDomain(persistenceBooking));
    }
}
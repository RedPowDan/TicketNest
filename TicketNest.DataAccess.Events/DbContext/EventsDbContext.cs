using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.Models;

namespace TicketNest.DataAccess.Events.DbContext;

internal sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<PersistenceEvent> Events { get; set; } = null!;
    
    public DbSet<PersistenceBooking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
    }
}
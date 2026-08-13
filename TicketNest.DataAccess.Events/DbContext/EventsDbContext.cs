using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.Models;

namespace TicketNest.DataAccess.Events.DbContext;

internal sealed class EventsDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<PersistenceEvent> Events { get; set; } = null!;
    
    public DbSet<PersistenceBooking> Bookings { get; set; } = null!;

    public DbSet<PersistenceUser> Users { get; set; } = null!;

    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
    }

    // Конструктор для design-time
    public EventsDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // для design-time
            optionsBuilder.UseNpgsql();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
    }
}
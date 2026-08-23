using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Bookings.Models;

namespace TicketNest.DataAccess.Bookings.DbContext;

internal sealed class BookingsDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<PersistenceBooking> Bookings { get; set; } = null!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    public BookingsDbContext(DbContextOptions<BookingsDbContext> options) : base(options)
    {
    }

    // Конструктор для design-time
    public BookingsDbContext()
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
    }
}
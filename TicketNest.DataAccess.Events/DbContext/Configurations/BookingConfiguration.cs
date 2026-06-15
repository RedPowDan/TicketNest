using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketNest.DataAccess.Events.Models;

namespace TicketNest.DataAccess.Events.DbContext.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<PersistenceBooking>
{
    public void Configure(EntityTypeBuilder<PersistenceBooking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.Status).HasConversion<string>();
    }
}
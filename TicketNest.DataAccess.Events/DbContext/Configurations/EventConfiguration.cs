using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketNest.DataAccess.Events.Models;

namespace TicketNest.DataAccess.Events.DbContext.Configurations;

internal sealed class EventConfiguration : IEntityTypeConfiguration<PersistenceEvent>
{
    public void Configure(EntityTypeBuilder<PersistenceEvent> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).IsRequired();

        builder
            .HasMany(x => x.Bookings)
            .WithOne(x => x.Event)
            .HasForeignKey(x => x.EventId);
    }
}
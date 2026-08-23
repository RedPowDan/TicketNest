using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketNest.DataAccess.Auth.Models;

namespace TicketNest.DataAccess.Auth.DbContext.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<PersistenceUser>
{
    public void Configure(EntityTypeBuilder<PersistenceUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Role).HasConversion<string>();

        builder.HasIndex(u => u.Login).IsUnique();
    }
}
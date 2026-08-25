using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Auth.Models;

namespace TicketNest.DataAccess.Auth.DbContext;

internal sealed class AuthDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<PersistenceUser> Users { get; set; } = null!;

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    // Конструктор для design-time
    public AuthDbContext()
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
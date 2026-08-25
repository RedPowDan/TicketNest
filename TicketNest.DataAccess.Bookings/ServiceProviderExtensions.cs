using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Bookings.DbContext;

namespace TicketNest.DataAccess.Bookings;

public static class ServiceProviderExtensions
{
    public static void RunMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();

        db.Database.Migrate();
    }
}
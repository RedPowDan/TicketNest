using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Events1.DbContext;

namespace TicketNest.DataAccess.Events1;

public static class ServiceProviderExtensions
{
    public static void RunMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();

        db.Database.Migrate();
    }
}
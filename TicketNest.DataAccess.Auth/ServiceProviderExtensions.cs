using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Auth.DbContext;

namespace TicketNest.DataAccess.Auth;

public static class ServiceProviderExtensions
{
    public static void RunMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        db.Database.Migrate();
    }
}
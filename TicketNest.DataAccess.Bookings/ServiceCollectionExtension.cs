using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Bookings.DbContext;
using TicketNest.DataAccess.Bookings.Implementations;
using TicketNest.Domain.Bookings.Repositories;

namespace TicketNest.DataAccess.Bookings;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBookingAccess(this IServiceCollection services, string connectionString)
    {
        return services
                .AddDbContext<BookingsDbContext>(options => options.UseNpgsql(connectionString))
                .AddScoped<IBookingRepository, BookingRepository>()
            ;
    }
}
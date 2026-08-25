using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TicketNest.DataAccess.Bookings.DbContext;
using TicketNest.DataAccess.Bookings.Implementations;
using TicketNest.DataAccess.Bookings.Outbox;
using TicketNest.Domain.Bookings.Repositories;

namespace TicketNest.DataAccess.Bookings;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBookingAccess(this IServiceCollection services, string connectionString)
    {
        return services
                .AddDbContext<BookingsDbContext>(options => options.UseNpgsql(connectionString))
                .AddScoped<IBookingRepository, BookingRepository>()
                .AddScoped<IOutboxRepository, OutboxRepository>()
            ;
    }
}

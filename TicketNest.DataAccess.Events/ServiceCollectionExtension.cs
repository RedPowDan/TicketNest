using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Events.DbContext;
using TicketNest.DataAccess.Events.Implementations;
using TicketNest.Domain.Events.Repositories;

namespace TicketNest.DataAccess.Events;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEventDataAccess(this IServiceCollection services, string connectionString)
    {
        return services
                .AddDbContext<EventsDbContext>(options => options.UseNpgsql(connectionString))
                .AddScoped<IEventsRepository, EventRepository>()
            ;
    }
}
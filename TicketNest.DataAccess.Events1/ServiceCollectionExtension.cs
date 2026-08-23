using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Events1.DbContext;
using TicketNest.DataAccess.Events1.Implementations;
using TicketNest.Domain.Events.Repositories;

namespace TicketNest.DataAccess.Events1;

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
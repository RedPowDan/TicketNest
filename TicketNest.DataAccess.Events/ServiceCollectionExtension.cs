using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Events.Implementations;
using TicketNest.Domain.Repositories;

namespace TicketNest.DataAccess.Events;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEventDataAccess(this IServiceCollection services)
    {
        return services.AddScoped<IEventsRepository, EventRepository>();
    }
}
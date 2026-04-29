using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.Services.Events;

namespace TicketNest.Application;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IEventService, EventService>();
    }
}
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Queue.Implementations;
using TicketNest.Domain.Repositories;

namespace TicketNest.DataAccess.Queue;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddQueueDataAccess(this IServiceCollection services)
    {
        return services
            .AddScoped<IQueueMessageRepository, MemoryQueueMessageRepository>()
            ;
    }
}
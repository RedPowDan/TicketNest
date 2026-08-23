using Microsoft.Extensions.DependencyInjection;
using TicketNest.Domain.Bookings.Services;

namespace TicketNest.Queues;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddQueues(this IServiceCollection services)
    {
        return services
                .AddScoped<IBookingProducer, BookingProducer>()
            ;
    }
}

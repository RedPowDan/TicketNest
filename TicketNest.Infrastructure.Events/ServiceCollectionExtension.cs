using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TicketNest.Application.Events.Cache;
using TicketNest.Infrastructure.Events.Cache;

namespace TicketNest.Infrastructure.Events;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CacheSettings>>().Value;
            return ConnectionMultiplexer.Connect(settings.ConnectionString);
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using TicketNest.Application.Auth.Services.Users;
using TicketNest.Domain.Auth.Services.Users;

namespace TicketNest.Application.Auth;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
                .AddDomainServices()
                .AddScoped<IUserService, UserService>()
            ;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        return services
                .AddScoped<IUserFactory, UserFactory>()
            ;
    }
}
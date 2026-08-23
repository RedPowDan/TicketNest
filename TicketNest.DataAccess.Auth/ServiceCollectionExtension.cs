using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.DataAccess.Auth.DbContext;
using TicketNest.DataAccess.Auth.Implementations;
using TicketNest.Domain.Auth.Repositories;

namespace TicketNest.DataAccess.Auth;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEventDataAccess(this IServiceCollection services, string connectionString)
    {
        return services
                .AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString))
                .AddScoped<IUserRepository, UserRepository>()
            ;
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketNest.Domain.Services.Auth;
using TicketNest.Infrastructure.Auth;

namespace TicketNest.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
                .Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName))
                .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
                .AddScoped<IJwtTokenReader, JwtTokenReader>();
    }
}

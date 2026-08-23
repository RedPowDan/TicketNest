using TicketNest.Application.Bookings;
using TicketNest.Bookings.Api.DI;
using TicketNest.Bookings.Api.Infrastructure;
using TicketNest.Bookings.Api.Middlewares;
using TicketNest.Bookings.Api.Services;
using TicketNest.DataAccess.Bookings;

namespace TicketNest.Bookings.Api;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting(options => 
        {
            options.LowercaseUrls = true;
        });
        services.AddControllers();
        services.AddSwagger();
        services.AddApplicationServices();
        services.AddBookingAccess(Configuration.GetConnectionString("EventsDbConnection")!);
        //services.AddQueueDataAccess();
        //services.AddInfrastructure(Configuration);
        services.AddScoped<IJwtTokenReader, JwtTokenReader>();
        services.AddScoped<ExceptionHandlingMiddleware>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddJwt(Configuration);

        services.AddAuthorization();

        services
            .AddMvc()
            .AddNewtonsoftJson(options => JsonSettingsConfigurator.ConfigureSettings(options.SerializerSettings));
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketNest API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Services.RunMigrations();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
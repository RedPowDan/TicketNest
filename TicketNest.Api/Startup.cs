using Microsoft.EntityFrameworkCore;
using TicketNest.Api.DI;
using TicketNest.Api.Infrastructure;
using TicketNest.Api.Middlewares;
using TicketNest.Application;
using TicketNest.DataAccess.Events;
using TicketNest.DataAccess.Queue;
using TicketNest.Infrastructure;

namespace TicketNest.Api;

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
        services.AddEventDataAccess(Configuration.GetConnectionString("EventsDbConnection")!);
        services.AddQueueDataAccess();
        services.AddInfrastructure(Configuration);
        services.AddScoped<ExceptionHandlingMiddleware>();

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

        app.Services.RunMigrations();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
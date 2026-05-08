using TicketNest.Api.DI;
using TicketNest.Api.Infrastructure;
using TicketNest.Api.Middlewares;
using TicketNest.Application;
using TicketNest.DataAccess.Events;

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
        services.AddEventDataAccess();
        services.AddScoped<ExceptionHandlingMiddleware>();

        services
            .AddMvc()
            .AddNewtonsoftJson(options => JsonSettingsConfigurator.ConfigureSettings(options.SerializerSettings));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
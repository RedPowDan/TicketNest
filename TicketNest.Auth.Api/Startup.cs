using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TicketNest.Application.Auth;
using TicketNest.Auth.Api.Infrastructure;
using TicketNest.Auth.Api.Middlewares;
using TicketNest.DataAccess.Auth;
using TicketNest.Infrastructure;
using TicketNest.Auth.Api.DI;

namespace TicketNest.Auth.Api;

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
        services.AddEventDataAccess(Configuration.GetConnectionString("AuthDbConnection")!);
        services.AddInfrastructure(Configuration);
        services.AddScoped<ExceptionHandlingMiddleware>();
        services.AddHttpContextAccessor();
        services.AddJwt(Configuration);
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName: "users-service"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(Configuration["Otlp:Endpoint"]!)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter())
            ;

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

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapPrometheusScrapingEndpoint();  // доступен по /metrics
        });
    }
}
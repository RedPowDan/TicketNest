using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TicketNest.Application.Bookings;
using TicketNest.Bookings.Api.DI;
using TicketNest.Contracts.Kafka;
using TicketNest.Bookings.Api.Infrastructure;
using TicketNest.Bookings.Api.Middlewares;
using TicketNest.Bookings.Api.Services;
using TicketNest.DataAccess.Bookings;
using TicketNest.Kafka;
using TicketNest.Queues.Bookings;

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
        services.AddQueues(Configuration);
        services.AddBookingAccess(Configuration.GetConnectionString("EventsDbConnection")!);
        //services.AddQueueDataAccess();
        //services.AddInfrastructure(Configuration);
        services.AddScoped<IJwtTokenReader, JwtTokenReader>();
        services.AddScoped<ExceptionHandlingMiddleware>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddJwt(Configuration);
        services.AddKafkaInfrastructure();
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(Configuration["Otlp:Endpoint"]!)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter())
            .ConfigureResource(r => r.AddService(serviceName: "bookings-service"));

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

        KafkaTopicInitializer.EnsureTopicsCreated(
            Configuration["Kafka:BaseUrl"]!,
            Configuration["Kafka:Login"],
            Configuration["Kafka:Password"],
            KafkaTopics.BookingTopic,
            KafkaTopics.EventTopic);

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        app.MapPrometheusScrapingEndpoint(); // доступен по /metrics 
    }
}
using TicketNest.Application.Events;
using TicketNest.Contracts.Kafka;
using TicketNest.DataAccess.Events;
using TicketNest.Events.Api.DI;
using TicketNest.Events.Api.Infrastructure;
using TicketNest.Events.Api.Middlewares;
using TicketNest.Kafka;
using TicketNest.Queues.Events;

namespace TicketNest.Events.Api;

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
        services.AddScoped<ExceptionHandlingMiddleware>();
        services.AddHttpContextAccessor();
        services.AddQueues(Configuration);
        services.AddJwt(Configuration);
        services.AddKafkaInfrastructure();

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
    }
}
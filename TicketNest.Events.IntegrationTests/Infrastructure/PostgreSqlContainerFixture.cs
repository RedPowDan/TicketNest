using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TicketNest.DataAccess.Auth.DbContext;
using TicketNest.DataAccess.Bookings.DbContext;
using TicketNest.DataAccess.Events.DbContext;

namespace TicketNest.Events.IntegrationTests.Infrastructure;

public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:14-alpine")
            .Build();
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await MigrateAsync<EventsDbContext>(ConnectionString);
        await MigrateAsync<BookingsDbContext>(ConnectionString);
        await MigrateAsync<AuthDbContext>(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
            _container = null;
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await using var eventsContext = new EventsDbContext(
            new DbContextOptionsBuilder<EventsDbContext>().UseNpgsql(ConnectionString).Options);
        await eventsContext.Events.ExecuteDeleteAsync();

        await using var bookingsContext = new BookingsDbContext(
            new DbContextOptionsBuilder<BookingsDbContext>().UseNpgsql(ConnectionString).Options);
        await bookingsContext.Bookings.ExecuteDeleteAsync();
        await bookingsContext.OutboxMessages.ExecuteDeleteAsync();

        await using var authContext = new AuthDbContext(
            new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(ConnectionString).Options);
        await authContext.Users.ExecuteDeleteAsync();
    }

    private static async Task MigrateAsync<TContext>(string connectionString)
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        await dbContext.Database.MigrateAsync();
    }
}

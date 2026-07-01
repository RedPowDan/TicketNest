using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TicketNest.DataAccess.Events.DbContext;

namespace TicketNest.IntegrationTests.Infrastructure;

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

        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var dbContext = new EventsDbContext(options);
        await dbContext.Database.MigrateAsync();
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
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var dbContext = new EventsDbContext(options);
        await dbContext.Bookings.ExecuteDeleteAsync();
        await dbContext.Events.ExecuteDeleteAsync();
    }
}

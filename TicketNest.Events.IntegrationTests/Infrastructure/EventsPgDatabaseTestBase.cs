using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.DbContext;

namespace TicketNest.Events.IntegrationTests.Infrastructure;

public abstract class EventsPgDatabaseTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _fixture;

    protected EventsPgDatabaseTestBase(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    internal EventsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new EventsDbContext(options);
    }
}

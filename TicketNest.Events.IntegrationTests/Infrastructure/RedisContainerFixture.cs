using StackExchange.Redis;
using Testcontainers.Redis;

namespace TicketNest.Events.IntegrationTests.Infrastructure;

public sealed class RedisContainerFixture : IAsyncLifetime
{
    private RedisContainer? _container;

    public string ConnectionString { get; private set; } = null!;

    public IConnectionMultiplexer Multiplexer { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new RedisBuilder("redis:7.2-alpine")
            .Build();
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
        Multiplexer = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (Multiplexer != null)
        {
            await Multiplexer.CloseAsync();
            await Multiplexer.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.DisposeAsync();
            _container = null;
        }
    }

    public async Task FlushAllAsync()
    {
        var db = Multiplexer.GetDatabase();
        await db.KeyDeleteAsync("*");
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TicketNest.Application.Events.Cache;
using TicketNest.Infrastructure.Events.Cache;
using TicketNest.Events.IntegrationTests.Infrastructure;

namespace TicketNest.Events.IntegrationTests.Cache;

[Collection("Redis")]
public class RedisCacheServiceTests : IAsyncLifetime
{
    private readonly RedisContainerFixture _fixture;
    private RedisCacheService _cacheService = null!;

    public RedisCacheServiceTests(RedisContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.FlushAllAsync();

        var settings = Options.Create(new CacheSettings
        {
            ConnectionString = _fixture.ConnectionString,
            IsEnabled = true
        });

        _cacheService = new RedisCacheService(
            _fixture.Multiplexer,
            settings,
            NullLogger<RedisCacheService>.Instance);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Get / Set

    [Fact]
    public async Task SetAsync_ThenGetAsync_ShouldReturnStoredValue()
    {
        var key = $"test:{Guid.NewGuid()}";
        var value = new TestDto { Id = 42, Name = "hello" };

        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestDto>(key);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.Name.Should().Be("hello");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_WhenKeyDoesNotExist()
    {
        var result = await _cacheService.GetAsync<TestDto>("nonexistent:key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithNull_ShouldStoreNull()
    {
        var key = $"test:{Guid.NewGuid()}";

        await _cacheService.SetAsync<string?>(key, null, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<string>(key);

        result.Should().BeNull();
    }

    #endregion

    #region Expiration

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_AfterExpiration()
    {
        var key = $"test:{Guid.NewGuid()}";
        var value = new TestDto { Id = 1, Name = "expiring" };

        await _cacheService.SetAsync(key, value, TimeSpan.FromSeconds(1));

        await Task.Delay(TimeSpan.FromSeconds(2));

        var result = await _cacheService.GetAsync<TestDto>(key);

        result.Should().BeNull();
    }

    #endregion

    #region Remove

    [Fact]
    public async Task RemoveAsync_ShouldDeleteKey()
    {
        var key = $"test:{Guid.NewGuid()}";
        var value = new TestDto { Id = 10, Name = "to-delete" };

        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var before = await _cacheService.GetAsync<TestDto>(key);
        before.Should().NotBeNull();

        await _cacheService.RemoveAsync(key);

        var after = await _cacheService.GetAsync<TestDto>(key);
        after.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldNotThrow_WhenKeyDoesNotExist()
    {
        var act = () => _cacheService.RemoveAsync("nonexistent:key");

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Overwrite

    [Fact]
    public async Task SetAsync_ShouldOverwriteExistingValue()
    {
        var key = $"test:{Guid.NewGuid()}";

        await _cacheService.SetAsync(key, new TestDto { Id = 1, Name = "first" }, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(key, new TestDto { Id = 2, Name = "second" }, TimeSpan.FromMinutes(5));

        var result = await _cacheService.GetAsync<TestDto>(key);

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("second");
    }

    #endregion

    #region Complex types

    [Fact]
    public async Task SetAsync_ShouldHandleListOfDtos()
    {
        var key = $"test:{Guid.NewGuid()}";
        var items = new List<TestDto>
        {
            new() { Id = 1, Name = "first" },
            new() { Id = 2, Name = "second" }
        };

        await _cacheService.SetAsync(key, items, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<List<TestDto>>(key);

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result[0].Name.Should().Be("first");
        result[1].Name.Should().Be("second");
    }

    #endregion

    #region Multiple keys

    [Fact]
    public async Task SetAsync_ShouldIsolateDifferentKeys()
    {
        var key1 = $"test:{Guid.NewGuid()}";
        var key2 = $"test:{Guid.NewGuid()}";

        await _cacheService.SetAsync(key1, new TestDto { Id = 1, Name = "first" }, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(key2, new TestDto { Id = 2, Name = "second" }, TimeSpan.FromMinutes(5));

        var result1 = await _cacheService.GetAsync<TestDto>(key1);
        var result2 = await _cacheService.GetAsync<TestDto>(key2);

        result1!.Name.Should().Be("first");
        result2!.Name.Should().Be("second");
    }

    [Fact]
    public async Task RemoveAsync_ShouldNotAffectOtherKeys()
    {
        var key1 = $"test:{Guid.NewGuid()}";
        var key2 = $"test:{Guid.NewGuid()}";

        await _cacheService.SetAsync(key1, new TestDto { Id = 1 }, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(key2, new TestDto { Id = 2 }, TimeSpan.FromMinutes(5));

        await _cacheService.RemoveAsync(key1);

        var result1 = await _cacheService.GetAsync<TestDto>(key1);
        var result2 = await _cacheService.GetAsync<TestDto>(key2);

        result1.Should().BeNull();
        result2.Should().NotBeNull();
    }

    #endregion

    #region IsEnabled

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_WhenCacheDisabled()
    {
        var disabledSettings = Options.Create(new CacheSettings
        {
            ConnectionString = _fixture.ConnectionString,
            IsEnabled = false
        });

        var disabledCache = new RedisCacheService(
            _fixture.Multiplexer,
            disabledSettings,
            NullLogger<RedisCacheService>.Instance);

        var key = $"test:{Guid.NewGuid()}";
        await disabledCache.SetAsync(key, new TestDto { Id = 1 }, TimeSpan.FromMinutes(5));

        var result = await disabledCache.GetAsync<TestDto>(key);
        result.Should().BeNull();
    }

    #endregion

    public class TestDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}

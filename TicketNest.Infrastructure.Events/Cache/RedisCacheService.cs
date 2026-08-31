using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TicketNest.Application.Events.Cache;

namespace TicketNest.Infrastructure.Events.Cache;

internal sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly CacheSettings _settings;

    public RedisCacheService(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<CacheSettings> settings,
        ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (!_settings.IsEnabled)
        {
            return default;
        }

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {Key}. Degraded to cache miss.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        if (!_settings.IsEnabled)
        {
            return;
        }

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serialized = JsonSerializer.Serialize(value);
            var ttl = expiration ?? TimeSpan.FromSeconds(_settings.EventByIdTtlSeconds);
            await db.StringSetAsync(key, serialized, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {Key}. Cache write skipped.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        if (!_settings.IsEnabled)
        {
            return;
        }

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis REMOVE failed for key {Key}. Cache invalidation skipped.", key);
        }
    }
}

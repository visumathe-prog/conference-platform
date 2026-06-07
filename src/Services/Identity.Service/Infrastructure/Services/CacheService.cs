using StackExchange.Redis;
using System.Text.Json;

namespace Conference.Identity.Infrastructure.Services
{
    /// <summary>
    /// Redis cache implementation for distributed caching
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        
        public RedisCacheService(
            IConnectionMultiplexer redis, 
            ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _logger = logger;
        }
        
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                
                if (value.IsNullOrEmpty)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }
                
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error while getting key: {Key}", key);
                // Fallback to default - don't crash the app
                return default;
            }
        }
        
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, json, expiration);
                _logger.LogDebug("Set cache for key: {Key}, expires in: {Expiration}", 
                    key, expiration);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error while setting key: {Key}", key);
                // Don't throw - caching failure shouldn't break the app
            }
        }
        
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                _logger.LogDebug("Removed cache key: {Key}", key);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error while removing key: {Key}", key);
            }
        }
        
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error while checking key: {Key}", key);
                return false;
            }
        }
        
        public async Task RefreshAsync(string key)
        {
            try
            {
                var ttl = await _database.KeyTimeToLiveAsync(key);
                if (ttl.HasValue && ttl.Value > TimeSpan.Zero)
                {
                    await _database.KeyExpireAsync(key, ttl.Value);
                    _logger.LogDebug("Refreshed cache key: {Key}", key);
                }
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error while refreshing key: {Key}", key);
            }
        }
    }
}

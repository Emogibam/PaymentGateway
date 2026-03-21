using PaymentGateway.Domain.Interfaces;
using StackExchange.Redis;

namespace PaymentGateway.Infrastructure.Services;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDatabase _database;

    public RedisIdempotencyService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<bool> IsDuplicateAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        await _database.StringSetAsync(key, value, expiry ?? TimeSpan.FromHours(24));
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _database.StringGetAsync(key);
    }
}

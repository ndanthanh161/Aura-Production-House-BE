using Aura.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Aura.Infrastructure.Services;

public class RedisTokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;
    private const string BlacklistPrefix = "token_blacklist:";

    public RedisTokenBlacklistService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration // Tự xóa khi token hết hạn
        };

        await _cache.SetStringAsync($"{BlacklistPrefix}{jti}", "revoked", options);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        var result = await _cache.GetStringAsync($"{BlacklistPrefix}{jti}");
        return result != null;
    }
}

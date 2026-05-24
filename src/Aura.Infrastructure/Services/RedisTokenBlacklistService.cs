using Aura.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Aura.Infrastructure.Services;

public class RedisTokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisTokenBlacklistService> _logger;
    private const string BlacklistPrefix = "token_blacklist:";

    public RedisTokenBlacklistService(IDistributedCache cache, ILogger<RedisTokenBlacklistService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan expiration)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration // Tự xóa khi token hết hạn
            };

            await _cache.SetStringAsync($"{BlacklistPrefix}{jti}", "revoked", options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to blacklist token in Redis. Bypassing.");
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        try
        {
            var result = await _cache.GetStringAsync($"{BlacklistPrefix}{jti}");
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check token blacklist in Redis. Bypassing.");
            return false; // Mặc định cho qua nếu Redis sập
        }
    }
}

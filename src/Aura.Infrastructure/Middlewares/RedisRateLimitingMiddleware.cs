using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;
using Aura.Application.Common;
using Microsoft.Extensions.Logging;

namespace Aura.Infrastructure.Middlewares
{
    public class RedisRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _redis;
        private const int Limit = 100; // Số request tối đa
        private const int WindowSeconds = 60; // Trong vòng 60 giây

        public RedisRateLimitingMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
        {
            _next = next;
            _redis = redis.GetDatabase();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.ToString().ToLower();

            // Chỉ giới hạn các API quan trọng hoặc toàn bộ API v1
            if (!path.StartsWith("/api/v1"))
            {
                await _next(context);
                return;
            }

            try
            {
                // Key: Aura_RateLimit:127.0.0.1:/api/v1/login
                var key = $"RateLimit:{ipAddress}:{path}";

                // Tăng biến đếm nguyên tử
                var count = await _redis.StringIncrementAsync(key);

                if (count == 1)
                {
                    // Nếu là request đầu tiên trong window, set thời gian hết hạn
                    await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(WindowSeconds));
                }

                if (count > Limit)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.ErrorResponse(
                        $"Too many requests. Please try again after {WindowSeconds} seconds.", 
                        429);

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }

                // Thêm Header để client biết giới hạn
                context.Response.Headers["X-Rate-Limit-Limit"] = Limit.ToString();
                context.Response.Headers["X-Rate-Limit-Remaining"] = (Limit - count).ToString();
            }
            catch (Exception ex)
            {
                // TRƯỜNG HỢP REDIS BỊ SẬP: Bỏ qua rate limit và cho phép request đi qua bình thường
                var logger = context.RequestServices.GetService(typeof(ILogger<RedisRateLimitingMiddleware>)) as ILogger<RedisRateLimitingMiddleware>;
                logger?.LogError(ex, "Redis connection failed in RateLimitingMiddleware. Bypassing check.");
            }

            await _next(context);
        }
    }
}

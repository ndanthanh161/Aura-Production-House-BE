using Aura.Application.Interfaces;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Aura.Infrastructure.Repositories;
using Aura.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Aura.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ===== PostgreSQL =====
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
                npgsqlOptions.CommandTimeout(30);
            });
        });

        // ===== Redis =====
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Aura_";
        });

        // ===== Repositories =====
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();

        // ===== Services =====
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>(); // ← THÊM
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IPhotographerService, PhotographerService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        // ===== JWT Authentication =====
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                // ===== CHECK BLACKLIST TRƯỚC KHI CHO PHÉP REQUEST =====
                OnTokenValidated = async context =>
                {
                    var blacklistService = context.HttpContext.RequestServices
                        .GetRequiredService<ITokenBlacklistService>();

                    var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                    if (!string.IsNullOrEmpty(jti) && await blacklistService.IsTokenBlacklistedAsync(jti))
                    {
                        context.Fail("Token has been revoked.");
                    }
                },

                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        success = false,
                        statusCode = 401,
                        message = "Unauthorized. Token is missing or invalid.",
                        timestamp = DateTime.UtcNow
                    };

                    return context.Response.WriteAsJsonAsync(response);
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        success = false,
                        statusCode = 403,
                        message = "Forbidden. You do not have permission to access this resource.",
                        timestamp = DateTime.UtcNow
                    };

                    return context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        return services;
    }
}

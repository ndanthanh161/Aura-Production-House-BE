using Aura.Application.Common;
using Aura.Application.Interfaces;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Aura.Infrastructure.Repositories;
using Aura.Infrastructure.Services;
using Aura.Domain.Settings;
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
                npgsqlOptions.UseVector(); // Kích hoạt pgvector
            });
        });

        // ===== Redis =====
        var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        
        // Cấu hình Redis linh hoạt hơn: Không sập app nếu lỗi kết nối ban đầu
        var redisOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisConn);
        redisOptions.AbortOnConnectFail = false; 
        redisOptions.ConnectRetry = 5;
        redisOptions.ConnectTimeout = 10000; // 10 giây

        var multiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisOptions);
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(multiplexer);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
            options.InstanceName = "Aura_";
        });

        // ===== Repositories =====
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
        services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
        services.AddScoped<IChatLogRepository, ChatLogRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();

        // ===== Services =====
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();

        // ===== Cloudinary =====
        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.AddScoped<ICloudinaryService, CloudinaryService>();


        // ===== Mail Service =====
        services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
        services.AddScoped<IMailService, MailService>();

        // ===== AI Chatbot (RAG) =====
        services.Configure<AiSettings>(configuration.GetSection("AiSettings"));
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

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

                    var response = ApiResponse<object>.UnauthorizedResponse("Unauthorized. Token is missing or invalid.");
                    return context.Response.WriteAsJsonAsync(response);
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.ForbiddenResponse("Forbidden. You do not have permission to access this resource.");
                    return context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        return services;
    }
}

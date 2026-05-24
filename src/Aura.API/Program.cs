using Aura.Application;
using Aura.Infrastructure;
using Aura.Infrastructure.Middlewares;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aura Production House API",
        Version = "v1",
        Description = "API for Aura Production House"
    });

    // JWT Bearer token in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() 
                    ?? new[] { "http://localhost:5173", "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Health Check
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Auto Migration & Seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<Aura.Infrastructure.Data.AppDbContext>();
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Found {Count} pending migrations: {Migrations}. Applying now...", pendingMigrations.Count, string.Join(", ", pendingMigrations));
            context.Database.Migrate();
            logger.LogInformation("Database migration completed successfully.");
        }
        else
        {
            logger.LogInformation("No pending database migrations found.");
        }
        await DataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "FATAL: An error occurred while migrating or seeding the database. Application startup stopped.");
        throw; // Rethrow to make container crash and expose the exact SQL exception in logs
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger in Development or if explicitly enabled via config
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts(); // Force HTTPS in production
}

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Dynamic connect-src based on environment
    var connectSrc = "connect-src 'self' https://*.azurecontainerapps.io https://auraproduction.com.vn https://*.ngrok-free.app";
    if (app.Environment.IsDevelopment())
    {
        connectSrc += " http://localhost:7283 https://localhost:7283";
    }

    context.Response.Headers.Append("Content-Security-Policy", $"default-src 'self'; script-src 'self' 'unsafe-inline' https://accounts.google.com; style-src 'self' 'unsafe-inline'; img-src 'self' data: https://res.cloudinary.com; font-src 'self'; {connectSrc};");
    await next();
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseMiddleware<RedisRateLimitingMiddleware>();
app.UseAuthentication();  // Before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

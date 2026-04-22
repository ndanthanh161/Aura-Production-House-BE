using Aura.Domain.Entity;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        // ==================== Seed Roles ====================
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Admin",
                    Description = "Full system access",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Staff",
                    Description = "Manage assigned projects",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "User",
                    Description = "Regular customer",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Photographer",
                    Description = "Capture moments",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // ==================== Seed Admin ====================
        if (!await context.Users.AnyAsync(u => u.Email == "admin@auraproduction.com"))
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),
                FullName = "System Admin",
                Email = "admin@auraproduction.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Phone = "0901234567",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(admin);
        }

        // ==================== Seed Staff ====================
        if (!await context.Users.AnyAsync(u => u.Email == "staff@auraproduction.com"))
        {
            var staff = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Marcus Valerius",
                Email = "staff@auraproduction.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                RoleId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Phone = "0909876543",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(staff);
        }

        await context.SaveChangesAsync();
    }
}

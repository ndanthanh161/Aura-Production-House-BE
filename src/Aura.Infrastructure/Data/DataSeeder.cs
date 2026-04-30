using Aura.Domain.Entity;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Aura.Application.Interfaces;

namespace Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

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
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "User",
                    Description = "Khách hàng sử dụng dịch vụ",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Photographer",
                    Description = "Thợ chụp ảnh chuyên nghiệp",
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

        // ==================== Seed Photographer ====================
        if (!await context.Users.AnyAsync(u => u.Email == "photo@auraproduction.com"))
        {
            var photographer = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Aura Photographer",
                Email = "photo@auraproduction.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Photo@123"),
                RoleId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Phone = "0908888888",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(photographer);
        }
        await context.SaveChangesAsync();

        // ==================== Seed AI Knowledge ====================
        if (!await context.AuraKnowledge.AnyAsync())
        {
            var packages = await context.Packages.ToListAsync();
            foreach (var pkg in packages)
            {
                var benefits = string.Join(", ", pkg.Benefits);
                var content = $"Gói dịch vụ: {pkg.Name}. Giá: {pkg.Price:N0} VNĐ. Mô tả: {pkg.Description}. Quyền lợi: {benefits}.";
                await chatService.IngestKnowledgeAsync(content, "Package");
            }

            // FAQ cơ bản
            await chatService.IngestKnowledgeAsync("Aura Production House tọa lạc tại 123 Đường ABC, Quận 1, TP. Hồ Chí Minh.", "FAQ");
            await chatService.IngestKnowledgeAsync("Thời gian làm việc của Aura là từ 8:00 đến 21:00 tất cả các ngày trong tuần.", "FAQ");
            await chatService.IngestKnowledgeAsync("Để đặt lịch, khách hàng cần thanh toán đặt cọc 50% giá trị gói dịch vụ.", "FAQ");
        }

        await context.SaveChangesAsync();
    }
}

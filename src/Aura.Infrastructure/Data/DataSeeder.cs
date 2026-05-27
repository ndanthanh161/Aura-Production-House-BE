using Aura.Domain.Entity;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Aura.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Aura.Infrastructure.Data;

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
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var adminEmail = config["AdminSettings:Email"];
        var adminPassword = config["AdminSettings:Password"];

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Admin",
                    Email = adminEmail,
                    Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Phone = "0901234567",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }
        }

        await context.SaveChangesAsync();

        // ==================== Seed AI Knowledge ====================
        // Logic thông minh: Kiểm tra từng Package, nếu chưa có trong AI Knowledge thì mới nạp
        var currentPackages = await context.Packages.ToListAsync();
        var existingKnowledge = await context.AuraKnowledge
            .Where(k => k.Category == "Package")
            .Select(k => k.Content)
            .ToListAsync();

        foreach (var pkg in currentPackages)
        {
            var benefits = string.Join(", ", pkg.Benefits);
            var content = $"Gói dịch vụ: {pkg.Name}. Giá: {pkg.Price:N0} VNĐ. Mô tả: {pkg.Description}. Quyền lợi: {benefits}.";
            
            // Nếu nội dung này chưa tồn tại trong bảng AI Knowledge thì mới nạp vào
            if (!existingKnowledge.Any(k => k.Contains($"Gói dịch vụ: {pkg.Name}")))
            {
                await chatService.IngestKnowledgeAsync(content, "Package");
            }
        }

        // Tương tự cho FAQ cơ bản
        if (!await context.AuraKnowledge.AnyAsync(k => k.Category == "FAQ"))
        {
            await chatService.IngestKnowledgeAsync("Aura Production House tọa lạc tại Lô E2a-7, Đường D1, Đ. Võ Chí Công, Long Thạnh Mỹ, Thành Phố Thủ Đức, Hồ Chí Minh.", "FAQ");
            await chatService.IngestKnowledgeAsync("Thời gian làm việc của Aura là từ 8:00 đến 21:00 tất cả các ngày trong tuần.", "FAQ");
            await chatService.IngestKnowledgeAsync("Để đặt lịch, khách hàng cần thanh toán đặt cọc 50% giá trị gói dịch vụ.", "FAQ");
        }

        await context.SaveChangesAsync();
    }
}

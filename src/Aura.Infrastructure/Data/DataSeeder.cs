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
        
        // ==================== Seed Packages ====================
        if (!await context.Packages.AnyAsync())
        {
            var packages = new List<Package>
            {
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Gói Khởi Đầu",
                    Price = 2000000,
                    Description = "Phù hợp cho cá nhân mới bắt đầu xây dựng hình ảnh.",
                    Benefits = new List<string> { "Chụp 1 buổi", "Chỉnh sửa 10 ảnh", "1 video ngắn" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Gói Chuyên Nghiệp",
                    Price = 5000000,
                    Description = "Dành cho những người muốn làm thương hiệu cá nhân bài bản.",
                    Benefits = new List<string> { "Chụp 2 buổi", "Chỉnh sửa 30 ảnh", "3 video ngắn", "Trang điểm chuyên nghiệp" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Gói Đột Phá",
                    Price = 10000000,
                    Description = "Giải pháp toàn diện để trở thành Tech Influencer.",
                    Benefits = new List<string> { "Quản trị nội dung 1 tháng", "12 video TikTok/Reels", "Hỗ trợ kịch bản", "Chụp ảnh profile cao cấp" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await context.Packages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
        }

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
            await chatService.IngestKnowledgeAsync("Aura Production House tọa lạc tại 123 Đường ABC, Quận 1, TP. Hồ Chí Minh.", "FAQ");
            await chatService.IngestKnowledgeAsync("Thời gian làm việc của Aura là từ 8:00 đến 21:00 tất cả các ngày trong tuần.", "FAQ");
            await chatService.IngestKnowledgeAsync("Để đặt lịch, khách hàng cần thanh toán đặt cọc 50% giá trị gói dịch vụ.", "FAQ");
        }

        await context.SaveChangesAsync();
    }
}

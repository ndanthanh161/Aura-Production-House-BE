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
        
        // ==================== Seed Packages ====================
        if (!await context.Packages.AnyAsync())
        {
            var packages = new List<Package>
            {
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "CƠ BẢN",
                    Price = 2000000,
                    Description = "Dành cho cá nhân mới bắt đầu làm hình ảnh.",
                    Benefits = new List<string> { "1 buổi chụp (Profile hoặc sản phẩm)", "2 video (short video)" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "NÂNG CAO",
                    Price = 5000000,
                    Description = "Dành cho người muốn xây dựng thương hiệu bài bản.",
                    Benefits = new List<string> { "Lên kế hoạch chi tiết", "01 buổi chụp", "5 video", "Hỗ trợ chỉnh sửa kịch bản cá nhân hóa" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "TĂNG TỐC",
                    Price = 8000000,
                    Description = "Dành cho shop bán hàng hoặc KOLs đang lên.",
                    Benefits = new List<string> { "Lên kế hoạch chi tiết", "01 Concept chụp sáng tạo", "8 Video", "Hỗ trợ chỉnh sửa kịch bản cá nhân hóa", "Quản trị trang (Post bài/Set quảng cáo cơ bản)" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "ĐỘT PHÁ",
                    Price = 10000000,
                    Description = "Tư vấn định vị thương hiệu mạnh mẽ.",
                    Benefits = new List<string> { "Lên kế hoạch chi tiết", "1 Concept chụp sáng tạo", "12 video/tháng", "Hỗ trợ chỉnh sửa kịch bản cá nhân hóa", "Quản trị trang (Post bài/Set quảng cáo cơ bản)", "Tư vấn định vị thương hiệu" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "CHIẾN LƯỢC",
                    Price = 20000000,
                    Description = "Đối tác đồng hành dài hạn và chuyên sâu.",
                    Benefits = new List<string> { "Báo cáo hiệu quả hàng tháng", "Quản trị trang chuyên sâu", "Lên kế hoạch chi tiết", "1 Concept chụp độc quyền", "15 video/tháng", "Hỗ trợ chỉnh sửa kịch bản cá nhân hóa", "Tư vấn định vị thương hiệu" },
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

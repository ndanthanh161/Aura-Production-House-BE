using Aura.Application.DTOs.Statistics;
using Aura.Application.Interfaces;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Aura.Infrastructure.Data;

namespace Aura.Infrastructure.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        // ─── 1. Thống kê tổng quan dashboard ──────────────────────────────
        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstDayLastMonth = firstDayOfMonth.AddMonths(-1);

            // 1. Dữ liệu thô
            var projects = await _context.Projects
                .Include(p => p.Package)
                .ToListAsync();
            
            var payments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .ToListAsync();

            // 2. Tính toán doanh thu
            var totalRevenue = payments.Sum(p => p.Amount);
            var revenueThisMonth = payments.Where(p => p.CreatedAt >= firstDayOfMonth).Sum(p => p.Amount);
            var revenueLastMonth = payments.Where(p => p.CreatedAt >= firstDayLastMonth && p.CreatedAt < firstDayOfMonth).Sum(p => p.Amount);

            // 3. Phân tích tăng trưởng & AOV
            double revenueGrowth = 0;
            if (revenueLastMonth > 0)
            {
                revenueGrowth = (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100);
            }

            var paidProjectIds = payments.Select(p => p.ProjectId).Distinct().ToList();
            decimal averageOrderValue = paidProjectIds.Count > 0 ? totalRevenue / paidProjectIds.Count : 0;

            // 4. Tỉ lệ chuyển đổi (Conversion Rate)
            int totalBooked = projects.Count;
            int totalPaid = paidProjectIds.Count;
            double conversionRate = totalBooked > 0 ? (double)totalPaid / totalBooked * 100 : 0;

            // 5. Phân bổ theo Package
            var revenueByPackage = payments
                .GroupBy(p => p.Project?.Package?.Name ?? "Dịch vụ khác")
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var projectsByPackage = projects
                .GroupBy(p => p.Package?.Name ?? "Dịch vụ khác")
                .ToDictionary(g => g.Key, g => g.Count());

            // 6. Thống kê người dùng
            var userRoleId = await _context.Roles.Where(r => r.Name == "User").Select(r => r.Id).FirstOrDefaultAsync();
            var photographerRoleId = await _context.Roles.Where(r => r.Name == "Photographer").Select(r => r.Id).FirstOrDefaultAsync();
            
            var totalCustomers = await _context.Users.CountAsync(u => u.RoleId == userRoleId);
            var totalPhotographers = await _context.Users.CountAsync(u => u.RoleId == photographerRoleId);
            var newCustomersThisMonth = await _context.Users.CountAsync(u => u.RoleId == userRoleId && u.CreatedAt >= firstDayOfMonth);

            return new DashboardStatsDTO
            {
                // Projects
                TotalProjects        = projects.Count,
                ProjectsInProduction = projects.Count(p => p.Status == ProjectStatus.InProduction),
                ProjectsScheduled    = projects.Count(p => p.Status == ProjectStatus.Scheduled),
                ProjectsCompleted    = projects.Count(p => p.Status == ProjectStatus.Completed),
                ProjectsCancelled    = projects.Count(p => p.Status == ProjectStatus.Cancelled),
                
                // Revenue & Growth
                TotalRevenue         = totalRevenue,
                RevenueThisMonth     = revenueThisMonth,
                RevenueLastMonth     = revenueLastMonth,
                RevenueGrowth        = Math.Round(revenueGrowth, 1),
                AverageOrderValue    = Math.Round(averageOrderValue, 0),

                // Analysis
                ConversionRate       = Math.Round(conversionRate, 1),
                RevenueByPackage     = revenueByPackage,
                ProjectsByCategory   = projectsByPackage, // Using Package breakdown as Category breakdown

                // Users
                TotalCustomers       = totalCustomers,
                TotalStaff           = totalPhotographers, 
                NewCustomersThisMonth = newCustomersThisMonth,

                // Bookings
                TotalBookings        = projects.Count(p => p.Status != ProjectStatus.Cancelled),
                BookingsThisMonth    = projects.Count(p => p.CreatedAt >= firstDayOfMonth),
                CancelledThisMonth   = projects.Count(p => p.Status == ProjectStatus.Cancelled && p.UpdatedAt >= firstDayOfMonth),
                
                TotalActivePackages  = await _context.Packages.CountAsync(p => p.IsActive),
                GeneratedAt          = DateTime.UtcNow
            };
        }

        // ─── 2. Doanh thu theo tháng ───────────────────────────────────────
        public async Task<IEnumerable<MonthlyRevenueDTO>> GetMonthlyRevenueAsync(int months = 12)
        {
            var cutoff = DateTime.UtcNow.AddMonths(-months + 1);
            var firstOfCutoff = new DateTime(cutoff.Year, cutoff.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var completedPayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed
                         && p.CreatedAt >= firstOfCutoff)
                .ToListAsync();

            var result = completedPayments
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new MonthlyRevenueDTO
                {
                    Year         = g.Key.Year,
                    Month        = g.Key.Month,
                    Revenue      = g.Sum(p => p.Amount),
                    ProjectCount = g.Select(p => p.ProjectId).Distinct().Count()
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToList();

            return result;
        }

        // ─── 3. Hiệu suất photographer ─────────────────────────────────────
        public async Task<IEnumerable<PhotographerPerformanceDTO>> GetPhotographerPerformanceAsync()
        {
            var photographerRoleId = await _context.Roles
                .Where(r => r.Name == "Photographer")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var photographers = await _context.Users
                .Where(u => u.RoleId == photographerRoleId)
                .ToListAsync();

            var projects = await _context.Projects
                .Where(p => p.StaffId != null)
                .ToListAsync();

            var result = photographers.Select(ph =>
            {
                var phProjects = projects.Where(p => p.StaffId == ph.Id).ToList();
                return new PhotographerPerformanceDTO
                {
                    PhotographerId      = ph.Id,
                    PhotographerName    = ph.FullName,
                    TotalAssigned = phProjects.Count,
                    Completed    = phProjects.Count(p => p.Status == ProjectStatus.Completed),
                    InProgress   = phProjects.Count(p =>
                        p.Status == ProjectStatus.InProduction ||
                        p.Status == ProjectStatus.Scheduled),
                    Cancelled    = phProjects.Count(p => p.Status == ProjectStatus.Cancelled),
                    TotalRevenue = phProjects
                        .Where(p => p.Status == ProjectStatus.Completed)
                        .Sum(p => p.Revenue)
                };
            })
            .OrderByDescending(s => s.Completed)
            .ToList();

            return result;
        }
    }
}

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

            // Dự án
            var projects = await _context.Projects.ToListAsync();
            var totalRevenue = projects
                .Where(p => p.Status == ProjectStatus.Completed)
                .Sum(p => p.Revenue);

            var revenueThisMonth = projects
                .Where(p => p.Status == ProjectStatus.Completed && p.UpdatedAt >= firstDayOfMonth)
                .Sum(p => p.Revenue);

            var revenueLastMonth = projects
                .Where(p => p.Status == ProjectStatus.Completed
                         && p.UpdatedAt >= firstDayLastMonth
                         && p.UpdatedAt < firstDayOfMonth)
                .Sum(p => p.Revenue);

            // Booking tháng này
            var bookingsThisMonth = projects.Count(p =>
                (p.Status == ProjectStatus.Scheduled || p.Status == ProjectStatus.InProduction)
                && p.CreatedAt >= firstDayOfMonth);

            var cancelledThisMonth = projects.Count(p =>
                p.Status == ProjectStatus.Cancelled && p.UpdatedAt >= firstDayOfMonth);

            // Người dùng
            var photographerRoleId = await _context.Roles
                .Where(r => r.Name == "Photographer")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var userRoleId = await _context.Roles
                .Where(r => r.Name == "User")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var totalPhotographers = await _context.Users.CountAsync(u => u.RoleId == photographerRoleId);
            var totalCustomers = await _context.Users.CountAsync(u => u.RoleId == userRoleId);
            var newCustomersThisMonth = await _context.Users
                .CountAsync(u => u.RoleId == userRoleId && u.CreatedAt >= firstDayOfMonth);

            // Gói dịch vụ
            var activePackages = await _context.Packages.CountAsync(p => p.IsActive);

            return new DashboardStatsDTO
            {
                TotalProjects        = projects.Count,
                ProjectsInProduction = projects.Count(p => p.Status == ProjectStatus.InProduction),
                ProjectsScheduled    = projects.Count(p => p.Status == ProjectStatus.Scheduled),
                ProjectsCompleted    = projects.Count(p => p.Status == ProjectStatus.Completed),
                ProjectsCancelled    = projects.Count(p => p.Status == ProjectStatus.Cancelled),
                TotalRevenue         = totalRevenue,
                RevenueThisMonth     = revenueThisMonth,
                RevenueLastMonth     = revenueLastMonth,
                TotalCustomers       = totalCustomers,
                TotalStaff           = totalPhotographers, // Giữ tên thuộc tính DTO nhưng gán giá trị photographer
                NewCustomersThisMonth = newCustomersThisMonth,
                TotalBookings        = projects.Count(p =>
                    p.Status == ProjectStatus.Scheduled || p.Status == ProjectStatus.InProduction),
                BookingsThisMonth    = bookingsThisMonth,
                CancelledThisMonth   = cancelledThisMonth,
                TotalActivePackages  = activePackages,
                GeneratedAt          = DateTime.UtcNow
            };
        }

        // ─── 2. Doanh thu theo tháng ───────────────────────────────────────
        public async Task<IEnumerable<MonthlyRevenueDTO>> GetMonthlyRevenueAsync(int months = 12)
        {
            var cutoff = DateTime.UtcNow.AddMonths(-months + 1);
            var firstOfCutoff = new DateTime(cutoff.Year, cutoff.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var completedProjects = await _context.Projects
                .Where(p => p.Status == ProjectStatus.Completed
                         && p.UpdatedAt >= firstOfCutoff)
                .ToListAsync();

            var result = completedProjects
                .GroupBy(p => new { p.UpdatedAt.Year, p.UpdatedAt.Month })
                .Select(g => new MonthlyRevenueDTO
                {
                    Year         = g.Key.Year,
                    Month        = g.Key.Month,
                    Revenue      = g.Sum(p => p.Revenue),
                    ProjectCount = g.Count()
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToList();

            return result;
        }

        // ─── 3. Hiệu suất photographer ─────────────────────────────────────
        public async Task<IEnumerable<StaffPerformanceDTO>> GetStaffPerformanceAsync()
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
                return new StaffPerformanceDTO
                {
                    StaffId      = ph.Id,
                    StaffName    = ph.FullName,
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

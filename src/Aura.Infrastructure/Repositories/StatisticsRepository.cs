using Aura.Domain.Entity;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly AppDbContext _context;
    public StatisticsRepository(AppDbContext context) => _context = context;

    public async Task<int> GetTotalProjectsCountAsync() => await _context.Projects.CountAsync();

    public async Task<int> GetProjectsCountByStatusAsync(ProjectStatus status) 
        => await _context.Projects.CountAsync(p => p.Status == status);

    public async Task<decimal> GetTotalRevenueAsync() 
        => await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount);

    public async Task<decimal> GetRevenueInDateRangeAsync(DateTime start, DateTime end)
        => await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.CreatedAt >= start && p.CreatedAt < end)
            .SumAsync(p => p.Amount);

    public async Task<Dictionary<string, decimal>> GetRevenueByPackageAsync()
        => await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .GroupBy(p => p.Project.Package.Name)
            .ToDictionaryAsync(g => g.Key ?? "Dịch vụ khác", g => g.Sum(p => p.Amount));

    public async Task<Dictionary<string, int>> GetProjectsByPackageAsync()
        => await _context.Projects
            .GroupBy(p => p.Package.Name)
            .ToDictionaryAsync(g => g.Key ?? "Dịch vụ khác", g => g.Count());

    public async Task<int> GetUserCountByRoleAsync(string roleName)
        => await _context.Users.CountAsync(u => u.Role.Name == roleName);

    public async Task<int> GetNewCustomersCountAsync(DateTime since)
    {
        return await _context.Users
            .CountAsync(u => u.Role.Name == "User" && u.CreatedAt >= since);
    }

    public async Task<int> GetPaidProjectsCountAsync()
    {
        return await _context.Payments
            .Select(p => p.ProjectId)
            .Distinct()
            .CountAsync();
    }

    public async Task<int> GetActivePackagesCountAsync()
    {
        return await _context.Packages
            .CountAsync(p => p.IsActive);
    }

    public async Task<IEnumerable<(int Year, int Month, decimal Revenue, int ProjectCount)>> GetMonthlyRevenueStatsAsync(int months)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months + 1);
        var firstOfCutoff = new DateTime(cutoff.Year, cutoff.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var stats = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.CreatedAt >= firstOfCutoff)
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new 
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(p => p.Amount),
                ProjectCount = g.Select(p => p.ProjectId).Distinct().Count()
            })
            .OrderBy(r => r.Year).ThenBy(r => r.Month)
            .ToListAsync();

        return stats.Select(s => (s.Year, s.Month, s.Revenue, s.ProjectCount));
    }

    public async Task<IEnumerable<(Guid PhotographerId, string PhotographerName, int TotalAssigned, int Completed, int InProgress, int Cancelled, decimal TotalRevenue)>> GetPhotographerPerformanceStatsAsync()
    {
        var stats = await _context.Users
            .Where(u => u.Role.Name == "Photographer")
            .Select(ph => new 
            {
                PhotographerId = ph.Id,
                PhotographerName = ph.FullName,
                TotalAssigned = _context.Projects.Count(p => p.StaffId == ph.Id),
                Completed = _context.Projects.Count(p => p.StaffId == ph.Id && p.Status == ProjectStatus.Completed),
                InProgress = _context.Projects.Count(p => p.StaffId == ph.Id && (p.Status == ProjectStatus.InProduction || p.Status == ProjectStatus.Scheduled)),
                Cancelled = _context.Projects.Count(p => p.StaffId == ph.Id && p.Status == ProjectStatus.Cancelled),
                TotalRevenue = _context.Projects
                    .Where(p => p.StaffId == ph.Id && p.Status == ProjectStatus.Completed)
                    .Sum(p => p.Revenue)
            })
            .OrderByDescending(s => s.Completed)
            .ToListAsync();

        return stats.Select(s => (s.PhotographerId, s.PhotographerName, s.TotalAssigned, s.Completed, s.InProgress, s.Cancelled, s.TotalRevenue));
    }
}

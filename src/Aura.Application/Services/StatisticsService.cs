using Aura.Application.DTOs.Statistics;
using Aura.Application.Interfaces;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;

namespace Aura.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticsRepository _statsRepo;

    public StatisticsService(IStatisticsRepository statsRepo)
    {
        _statsRepo = statsRepo;
    }

    public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayLastMonth = firstDayOfMonth.AddMonths(-1);

        var totalRevenue = await _statsRepo.GetTotalRevenueAsync();
        var revenueThisMonth = await _statsRepo.GetRevenueInDateRangeAsync(firstDayOfMonth, now.AddDays(1));
        var revenueLastMonth = await _statsRepo.GetRevenueInDateRangeAsync(firstDayLastMonth, firstDayOfMonth);
        var paidProjectCount = await _statsRepo.GetPaidProjectsCountAsync();
        var totalActivePackages = await _statsRepo.GetActivePackagesCountAsync();
        var totalCustomers = await _statsRepo.GetUserCountByRoleAsync("User");
        var totalPhotographers = await _statsRepo.GetUserCountByRoleAsync("Photographer");
        var newCustomersThisMonth = await _statsRepo.GetNewCustomersCountAsync(DateTime.UtcNow.AddMonths(-1));

        double revenueGrowth = revenueLastMonth > 0 ? (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100) : 0;

        return new DashboardStatsDTO
        {
            TotalRevenue = totalRevenue,
            RevenueThisMonth = revenueThisMonth,
            RevenueLastMonth = revenueLastMonth,
            TotalProjects = await _statsRepo.GetTotalProjectsCountAsync(),
            TotalCustomers = totalCustomers,
            TotalStaff = totalPhotographers, // Mapping photographer to staff for now
            AverageOrderValue = paidProjectCount > 0 ? totalRevenue / paidProjectCount : 0,
            NewCustomersThisMonth = newCustomersThisMonth,
            TotalActivePackages = totalActivePackages,
            ProjectsByCategory = await _statsRepo.GetProjectsByPackageAsync(),
            RevenueGrowth = Math.Round(revenueGrowth, 1),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<MonthlyRevenueDTO>> GetMonthlyRevenueAsync(int months = 12)
    {
        var stats = await _statsRepo.GetMonthlyRevenueStatsAsync(months);
        return stats.Select(s => new MonthlyRevenueDTO
        {
            Year = s.Year,
            Month = s.Month,
            Revenue = s.Revenue,
            ProjectCount = s.ProjectCount
        });
    }

    public async Task<IEnumerable<PhotographerPerformanceDTO>> GetPhotographerPerformanceAsync()
    {
        var stats = await _statsRepo.GetPhotographerPerformanceStatsAsync();
        return stats.Select(s => new PhotographerPerformanceDTO
        {
            PhotographerId = s.PhotographerId,
            PhotographerName = s.PhotographerName,
            TotalAssigned = s.TotalAssigned,
            Completed = s.Completed,
            InProgress = s.InProgress,
            Cancelled = s.Cancelled,
            TotalRevenue = s.TotalRevenue
        });
    }
}

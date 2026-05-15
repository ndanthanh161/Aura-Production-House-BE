using Aura.Domain.Entity;

namespace Aura.Domain.Interfaces;

public interface IStatisticsRepository
{
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueInDateRangeAsync(DateTime start, DateTime end);
    Task<int> GetTotalProjectsCountAsync();
    Task<Dictionary<string, int>> GetProjectsByPackageAsync();
    Task<int> GetUserCountByRoleAsync(string roleName);
    Task<int> GetNewCustomersCountAsync(DateTime since);
    Task<int> GetPaidProjectsCountAsync();
    Task<int> GetActivePackagesCountAsync();
    
    // Trả về Tuple thô, không dùng DTO
    Task<IEnumerable<(int Year, int Month, decimal Revenue, int ProjectCount)>> GetMonthlyRevenueStatsAsync(int months);
    Task<IEnumerable<(Guid PhotographerId, string PhotographerName, int TotalAssigned, int Completed, int InProgress, int Cancelled, decimal TotalRevenue)>> GetPhotographerPerformanceStatsAsync();
}

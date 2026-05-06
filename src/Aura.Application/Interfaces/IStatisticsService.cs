using Aura.Application.DTOs.Statistics;

namespace Aura.Application.Interfaces
{
    public interface IStatisticsService
    {
        /// <summary>Thống kê tổng quan dashboard</summary>
        Task<DashboardStatsDTO> GetDashboardStatsAsync();

        /// <summary>Doanh thu theo tháng (12 tháng gần nhất)</summary>
        Task<IEnumerable<MonthlyRevenueDTO>> GetMonthlyRevenueAsync(int months = 12);

        /// <summary>Hiệu suất từng photographer</summary>
        Task<IEnumerable<PhotographerPerformanceDTO>> GetPhotographerPerformanceAsync();
    }
}

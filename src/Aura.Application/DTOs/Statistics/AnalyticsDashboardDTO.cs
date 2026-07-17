namespace Aura.Application.DTOs.Statistics;

public class AnalyticsDashboardDTO
{
    public AnalyticsStatsDTO Dashboard { get; set; } = new();
    public OverviewStatsDTO Overview { get; set; } = new();
    public List<MonthlyRevenueDTO> MonthlyRevenue { get; set; } = new();
    public List<PackageRankingDTO> PackageRanking { get; set; } = new();
    public List<RecentPaymentDTO> RecentPayments { get; set; } = new();
    public List<PhotographerPerformanceDTO> PhotographerPerformance { get; set; } = new();
    public int Months { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
}

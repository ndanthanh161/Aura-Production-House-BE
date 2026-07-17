namespace Aura.Application.DTOs.Statistics;

public class AnalyticsStatsDTO : DashboardStatsDTO
{
    public decimal OutstandingAmount { get; set; }
    public int PaidProjects { get; set; }
}

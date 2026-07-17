namespace Aura.Application.DTOs.Statistics;

public class OverviewStatsDTO
{
    public decimal RevenueThisMonth { get; set; }
    public int ProjectsInProduction { get; set; }
    public int AwaitingPaymentProjects { get; set; }
    public int UnreadMessages { get; set; }
    public List<OverviewProjectDTO> UnassignedProjects { get; set; } = new();
    public List<OverviewProjectDTO> UpcomingProjects { get; set; } = new();
    public List<RecentPaymentDTO> RecentPayments { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class OverviewProjectDTO
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string? PhotographerName { get; set; }
    public DateTime Deadline { get; set; }
}

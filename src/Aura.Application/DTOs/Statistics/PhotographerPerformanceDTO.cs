namespace Aura.Application.DTOs.Statistics;

public class PhotographerPerformanceDTO
{
    public Guid PhotographerId { get; set; }
    public string PhotographerName { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public int Cancelled { get; set; }
    public decimal TotalRevenue { get; set; }
}

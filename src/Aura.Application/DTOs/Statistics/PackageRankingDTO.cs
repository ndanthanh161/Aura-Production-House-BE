namespace Aura.Application.DTOs.Statistics;

public class PackageRankingDTO
{
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public double RevenueShare { get; set; }
    public double Growth { get; set; }
    public DateTime? LastPurchasedAt { get; set; }
}

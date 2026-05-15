namespace Aura.Application.DTOs.Statistics;

public class MonthlyRevenueDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int ProjectCount { get; set; }
}

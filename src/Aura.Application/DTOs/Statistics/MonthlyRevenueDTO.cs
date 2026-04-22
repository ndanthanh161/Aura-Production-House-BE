namespace Aura.Application.DTOs.Statistics
{
    /// <summary>Doanh thu theo tháng (cho biểu đồ)</summary>
    public class MonthlyRevenueDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int ProjectCount { get; set; }
    }
}

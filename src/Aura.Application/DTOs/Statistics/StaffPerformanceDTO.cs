namespace Aura.Application.DTOs.Statistics
{
    /// <summary>Hiệu suất của từng nhân viên photographer</summary>
    public class StaffPerformanceDTO
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int Cancelled { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}

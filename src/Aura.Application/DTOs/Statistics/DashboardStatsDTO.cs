namespace Aura.Application.DTOs.Statistics
{
    /// <summary>Dashboard thống kê tổng quan cho Admin</summary>
    public class DashboardStatsDTO
    {
        // ── Dự án ──────────────────────────────
        public int TotalProjects { get; set; }
        public int ProjectsInProduction { get; set; }
        public int ProjectsScheduled { get; set; }
        public int ProjectsCompleted { get; set; }
        public int ProjectsCancelled { get; set; }

        // ── Doanh thu ──────────────────────────
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }

        // ── Người dùng ─────────────────────────
        public int TotalCustomers { get; set; }
        public int TotalStaff { get; set; }
        public int NewCustomersThisMonth { get; set; }

        // ── Booking ────────────────────────────
        public int TotalBookings { get; set; }             // Tổng lịch (Scheduled + InProduction)
        public int BookingsThisMonth { get; set; }
        public int CancelledThisMonth { get; set; }

        // ── Gói dịch vụ ───────────────────────
        public int TotalActivePackages { get; set; }

        // ── Thời gian tạo báo cáo ──────────────
        public DateTime GeneratedAt { get; set; }
    }
}

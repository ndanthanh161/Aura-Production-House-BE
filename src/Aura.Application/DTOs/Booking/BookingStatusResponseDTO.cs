using Aura.Domain.Enum;

namespace Aura.Application.DTOs.Booking
{
    /// <summary>
    /// Chi tiết trạng thái booking của một dự án
    /// </summary>
    public class BookingStatusResponseDTO
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;   // Nhãn tiếng Việt
        public DateTime ShootingDate { get; set; }
        public bool IsCancellable { get; set; }   // Còn trong thời gian cho phép hủy?
        public bool IsReschedulable { get; set; } // Còn trong thời gian cho phép đổi lịch?
        public DateTime UpdatedAt { get; set; }
    }
}

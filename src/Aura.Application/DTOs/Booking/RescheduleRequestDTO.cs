namespace Aura.Application.DTOs.Booking
{
    /// <summary>
    /// Request đổi lịch chụp — chỉ cần ID dự án và ngày mới
    /// </summary>
    public class RescheduleRequestDTO
    {
        public Guid ProjectId { get; set; }

        public DateTime NewShootingDate { get; set; }

        public string? Reason { get; set; }
    }
}

using Aura.Domain.Enum;

namespace Aura.Application.DTOs.Booking
{
    public class BookingScheduleResponseDTO
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public Guid PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public DateTime ShootingDate { get; set; }
        public ProjectStatus Status { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

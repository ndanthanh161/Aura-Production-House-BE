using Aura.Domain.Enum;

namespace Aura.Application.DTOs.Project
{
    public class UpdateProjectRequestDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid StaffId { get; set; } // Dùng cho Admin phân công Nhân viên dự án
        public ProjectStatus Status { get; set; }
        public decimal Revenue { get; set; } // Giá tiền thực tế (VD: sau khi Sale-off cho khách)
        public decimal Deposit { get; set; }
        public DateTime Deadline { get; set; }
        public string? Description { get; set; }
    }
}

using Aura.Domain.Enum;

namespace Aura.Application.DTOs.Project
{
    public class ProjectResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string? ClientName { get; set; }
        public Guid PackageId { get; set; }
        public string? PackageName { get; set; }
        public decimal Deposit { get; set; }
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public ProjectStatus Status { get; set; }
        public decimal Revenue { get; set; }
        public DateTime Deadline { get; set; }
        public string? Description { get; set; }
        public string? ResultLink { get; set; }

        /// <summary>
        /// Snapshot lợi ích được cam kết tại thời điểm customer mua package.
        /// </summary>
        public List<string> Benefits { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}



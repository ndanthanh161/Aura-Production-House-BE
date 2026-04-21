using Aura.Domain.Enum;

namespace Aura.Application.DTOs.Project
{
    public class ProjectResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public Guid PackageId { get; set; }
        public Guid? StaffId { get; set; }
        public ProjectStatus Status { get; set; }
        public decimal Revenue { get; set; }
        public DateTime Deadline { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


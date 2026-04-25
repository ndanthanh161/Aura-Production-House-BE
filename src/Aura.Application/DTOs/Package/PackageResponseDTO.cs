namespace Aura.Application.DTOs.Package
{
    public class PackageResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách lợi ích chi tiết để Frontend render dạng bullet list.
        /// </summary>
        public List<string> Benefits { get; set; } = new List<string>();

        public bool IsPopular { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


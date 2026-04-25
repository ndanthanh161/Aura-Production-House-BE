namespace Aura.Domain.Entity
{
    public class Package
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách lợi ích chi tiết mà customer nhận được khi mua package này.
        /// Được serialize thành JSON column trong DB.
        /// </summary>
        public List<string> Benefits { get; set; } = new List<string>();

        public bool IsPopular { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}

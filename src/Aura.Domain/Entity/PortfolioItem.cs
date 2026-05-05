using Aura.Domain.Enum;

namespace Aura.Domain.Entity
{
    public class PortfolioItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public PortfolioCategory Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Content { get; set; } // Rich text / article body
        public string? ClientName { get; set; } // Tên khách hàng (hiển thị trên portfolio)
        public Guid? ProjectId { get; set; }
        public bool IsPublished { get; set; }
        public int DisplayOrder { get; set; } // Thứ tự hiển thị
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Project? Project { get; set; }
        public ICollection<PortfolioMedia> MediaItems { get; set; } = new List<PortfolioMedia>();
    }
}

using Aura.Domain.Enum;

namespace Aura.Domain.Entity
{
    public class PortfolioItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public PortfolioCategory Category { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ProjectId { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public Project? Project { get; set; }
    }

}

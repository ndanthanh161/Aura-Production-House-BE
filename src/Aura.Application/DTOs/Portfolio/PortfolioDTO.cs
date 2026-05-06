using Aura.Domain.Enum;

namespace Aura.Application.DTOs.Portfolio
{
    // ─── Request DTOs ────────────────────────────────────────

    public class CreatePortfolioRequestDTO
    {
        public string Title { get; set; } = string.Empty;
        public PortfolioCategory Category { get; set; }
        public string? Content { get; set; }
        public string? ClientName { get; set; }
        public Guid? ProjectId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHot { get; set; }
    }

    public class UpdatePortfolioRequestDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public PortfolioCategory Category { get; set; }
        public string? Content { get; set; }
        public string? ClientName { get; set; }
        public Guid? ProjectId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHot { get; set; }
    }

    // ─── Response DTOs ───────────────────────────────────────

    public class PortfolioItemResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public PortfolioCategory Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Content { get; set; }
        public string? ClientName { get; set; }
        public Guid? ProjectId { get; set; }
        public bool IsPublished { get; set; }
        public bool IsHot { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PortfolioMediaResponseDTO> MediaItems { get; set; } = new();
    }

    public class PortfolioMediaResponseDTO
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string MediaType { get; set; } = "image";
        public int DisplayOrder { get; set; }
    }
}

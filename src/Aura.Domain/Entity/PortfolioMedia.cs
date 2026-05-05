namespace Aura.Domain.Entity
{
    public class PortfolioMedia
    {
        public Guid Id { get; set; }
        public Guid PortfolioItemId { get; set; }
        public string Url { get; set; } = string.Empty; // Cloudinary URL
        public string PublicId { get; set; } = string.Empty; // Cloudinary public_id (for deletion)
        public string MediaType { get; set; } = "image"; // "image" or "video"
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public PortfolioItem PortfolioItem { get; set; } = null!;
    }
}

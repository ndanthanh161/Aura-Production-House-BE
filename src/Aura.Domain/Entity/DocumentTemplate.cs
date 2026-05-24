using System;

namespace Aura.Domain.Entity
{
    public class DocumentTemplate
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty; // e.g. ".pdf", ".docx", ".doc"
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

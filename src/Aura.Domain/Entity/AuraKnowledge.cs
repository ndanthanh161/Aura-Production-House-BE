using Pgvector;

namespace Aura.Domain.Entity
{
    public class AuraKnowledge
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Vector Embedding { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}

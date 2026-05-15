using Aura.Domain.Entity;
using Pgvector;

namespace Aura.Domain.Interfaces;

public interface IKnowledgeRepository
{
    Task<IEnumerable<AuraKnowledge>> GetAllAsync();
    Task<AuraKnowledge?> GetByIdAsync(Guid id);
    Task AddAsync(AuraKnowledge knowledge);
    Task DeleteAsync(AuraKnowledge knowledge);
    Task<IEnumerable<string>> SearchRelevantContentAsync(Vector vector, int limit);
}

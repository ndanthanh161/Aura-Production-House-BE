using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories;

public class KnowledgeRepository : IKnowledgeRepository
{
    private readonly AppDbContext _context;
    public KnowledgeRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<AuraKnowledge>> GetAllAsync() => await _context.AuraKnowledge.ToListAsync();
    public async Task<AuraKnowledge?> GetByIdAsync(Guid id) => await _context.AuraKnowledge.FindAsync(id);
    
    public async Task AddAsync(AuraKnowledge knowledge) 
    { 
        await _context.AuraKnowledge.AddAsync(knowledge); 
        await _context.SaveChangesAsync(); 
    }

    public async Task DeleteAsync(AuraKnowledge knowledge) 
    { 
        _context.AuraKnowledge.Remove(knowledge); 
        await _context.SaveChangesAsync(); 
    }

    public async Task<IEnumerable<string>> SearchRelevantContentAsync(Vector vector, int limit)
    {
        return await _context.AuraKnowledge
            .OrderBy(k => k.Embedding.L2Distance(vector))
            .Take(limit)
            .Select(k => k.Content)
            .ToListAsync();
    }
}

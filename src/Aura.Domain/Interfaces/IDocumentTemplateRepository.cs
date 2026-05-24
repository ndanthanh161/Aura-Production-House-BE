using Aura.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aura.Domain.Interfaces
{
    public interface IDocumentTemplateRepository
    {
        Task<DocumentTemplate> AddAsync(DocumentTemplate template);
        Task<DocumentTemplate?> GetByIdAsync(Guid id);
        Task<IEnumerable<DocumentTemplate>> GetAllAsync(bool onlyPublished = false);
        Task<DocumentTemplate> UpdateAsync(DocumentTemplate template);
        Task<bool> DeleteAsync(Guid id);
    }
}

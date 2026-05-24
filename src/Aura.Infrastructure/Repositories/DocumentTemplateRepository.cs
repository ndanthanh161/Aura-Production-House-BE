using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aura.Infrastructure.Repositories
{
    public class DocumentTemplateRepository : IDocumentTemplateRepository
    {
        private readonly AppDbContext _context;

        public DocumentTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentTemplate> AddAsync(DocumentTemplate template)
        {
            await _context.DocumentTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<DocumentTemplate?> GetByIdAsync(Guid id)
        {
            return await _context.DocumentTemplates.FindAsync(id);
        }

        public async Task<IEnumerable<DocumentTemplate>> GetAllAsync(bool onlyPublished = false)
        {
            var query = _context.DocumentTemplates.AsQueryable();

            if (onlyPublished)
            {
                query = query.Where(d => d.IsPublished);
            }

            // Mới nhất xếp trên
            query = query.OrderByDescending(d => d.CreatedAt);

            return await query.ToListAsync();
        }

        public async Task<DocumentTemplate> UpdateAsync(DocumentTemplate template)
        {
            _context.DocumentTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var template = await _context.DocumentTemplates.FindAsync(id);
            if (template == null) return false;

            _context.DocumentTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

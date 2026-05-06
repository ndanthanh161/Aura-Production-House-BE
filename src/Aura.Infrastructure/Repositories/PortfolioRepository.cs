using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly AppDbContext _context;

        public PortfolioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PortfolioItem> AddAsync(PortfolioItem item)
        {
            _context.PortfolioItems.Add(item);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(item.Id) ?? item;
        }

        public async Task<PortfolioItem?> GetByIdAsync(Guid id)
        {
            return await _context.PortfolioItems
                .Include(p => p.MediaItems)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PortfolioItem>> GetAllAsync()
        {
            return await _context.PortfolioItems
                .Include(p => p.MediaItems)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PortfolioItem>> GetPublishedAsync()
        {
            return await _context.PortfolioItems
                .Where(p => p.IsPublished)
                .Include(p => p.MediaItems)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PortfolioItem> UpdateAsync(PortfolioItem item)
        {
            _context.PortfolioItems.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.PortfolioItems
                .Include(p => p.MediaItems)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (item == null) return false;

            _context.PortfolioMedias.RemoveRange(item.MediaItems);
            _context.PortfolioItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddMediaAsync(PortfolioMedia media)
        {
            _context.PortfolioMedias.Add(media);
            await _context.SaveChangesAsync();
        }

        public async Task<PortfolioMedia?> GetMediaByIdAsync(Guid mediaId)
        {
            return await _context.PortfolioMedias.FindAsync(mediaId);
        }

        public async Task<bool> DeleteMediaAsync(Guid mediaId)
        {
            var media = await _context.PortfolioMedias.FindAsync(mediaId);
            if (media == null) return false;

            _context.PortfolioMedias.Remove(media);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

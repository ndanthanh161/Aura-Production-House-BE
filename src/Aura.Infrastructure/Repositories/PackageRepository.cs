using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data; // Thay bằng namespace DbContext của bạn
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly AppDbContext _context;

        public PackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Package> AddAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<Package?> GetByIdAsync(Guid id)
        {
            return await _context.Packages.FindAsync(id);
        }

        public async Task<IEnumerable<Package>> GetAllAsync(bool onlyActive = true)
        {
            var query = _context.Packages.AsQueryable();

            if (onlyActive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query.ToListAsync();
        }

        public async Task<Package> UpdateAsync(Package package)
        {
            _context.Packages.Update(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null) return false;

            package.IsActive = false; // Soft-delete
            package.UpdatedAt = DateTime.UtcNow;

            _context.Packages.Update(package);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

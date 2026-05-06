using Aura.Domain.Entity;

namespace Aura.Domain.Interfaces
{
    public interface IPackageRepository
    {
        Task<Package> AddAsync(Package package);
        Task<Package?> GetByIdAsync(Guid id);
        Task<IEnumerable<Package>> GetAllAsync(bool onlyActive = true);
        Task<Package> UpdateAsync(Package package);
        Task<bool> DeleteAsync(Guid id); // Dùng cho Xóa mềm (Soft Delete)
    }
}

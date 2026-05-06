using Aura.Domain.Entity;

namespace Aura.Domain.Interfaces
{
    public interface IPortfolioRepository
    {
        Task<PortfolioItem> AddAsync(PortfolioItem item);
        Task<PortfolioItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<PortfolioItem>> GetAllAsync();
        Task<IEnumerable<PortfolioItem>> GetPublishedAsync();
        Task<PortfolioItem> UpdateAsync(PortfolioItem item);
        Task<bool> DeleteAsync(Guid id);
        Task AddMediaAsync(PortfolioMedia media);
        Task<bool> DeleteMediaAsync(Guid mediaId);
        Task<PortfolioMedia?> GetMediaByIdAsync(Guid mediaId);
    }
}

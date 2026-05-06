using Aura.Application.DTOs.Portfolio;
using Microsoft.AspNetCore.Http;

namespace Aura.Application.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioItemResponseDTO> CreateAsync(CreatePortfolioRequestDTO request);
        Task<PortfolioItemResponseDTO?> GetByIdAsync(Guid id);
        Task<IEnumerable<PortfolioItemResponseDTO>> GetAllAsync();
        Task<IEnumerable<PortfolioItemResponseDTO>> GetPublishedAsync();
        Task<PortfolioItemResponseDTO?> UpdateAsync(UpdatePortfolioRequestDTO request);
        Task<bool> TogglePublishAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<PortfolioMediaResponseDTO> UploadMediaAsync(Guid portfolioItemId, IFormFile file);
        Task<PortfolioMediaResponseDTO> AddMediaDirectAsync(Guid portfolioItemId, string url, string publicId, string mediaType);
        Task<bool> DeleteMediaAsync(Guid mediaId);
        object GetUploadSignature(string folder = "portfolio");
    }
}

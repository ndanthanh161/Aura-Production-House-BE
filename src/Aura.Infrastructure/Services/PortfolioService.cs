using Aura.Application.DTOs.Portfolio;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Aura.Infrastructure.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public PortfolioService(IPortfolioRepository portfolioRepository, ICloudinaryService cloudinaryService)
        {
            _portfolioRepository = portfolioRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PortfolioItemResponseDTO> CreateAsync(CreatePortfolioRequestDTO request)
        {
            var item = new PortfolioItem
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Category = request.Category,
                Content = request.Content,
                ClientName = request.ClientName,
                ProjectId = request.ProjectId,
                DisplayOrder = request.DisplayOrder,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _portfolioRepository.AddAsync(item);
            return MapToDTO(created);
        }

        public async Task<PortfolioItemResponseDTO?> GetByIdAsync(Guid id)
        {
            var item = await _portfolioRepository.GetByIdAsync(id);
            return item == null ? null : MapToDTO(item);
        }

        public async Task<IEnumerable<PortfolioItemResponseDTO>> GetAllAsync()
        {
            var items = await _portfolioRepository.GetAllAsync();
            return items.Select(MapToDTO);
        }

        public async Task<IEnumerable<PortfolioItemResponseDTO>> GetPublishedAsync()
        {
            var items = await _portfolioRepository.GetPublishedAsync();
            return items.Select(MapToDTO);
        }

        public async Task<PortfolioItemResponseDTO?> UpdateAsync(UpdatePortfolioRequestDTO request)
        {
            var item = await _portfolioRepository.GetByIdAsync(request.Id);
            if (item == null) return null;

            item.Title = request.Title;
            item.Category = request.Category;
            item.Content = request.Content;
            item.ClientName = request.ClientName;
            item.ProjectId = request.ProjectId;
            item.DisplayOrder = request.DisplayOrder;
            item.UpdatedAt = DateTime.UtcNow;

            var updated = await _portfolioRepository.UpdateAsync(item);
            return MapToDTO(updated);
        }

        public async Task<bool> TogglePublishAsync(Guid id)
        {
            var item = await _portfolioRepository.GetByIdAsync(id);
            if (item == null) return false;

            item.IsPublished = !item.IsPublished;
            item.UpdatedAt = DateTime.UtcNow;
            await _portfolioRepository.UpdateAsync(item);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _portfolioRepository.GetByIdAsync(id);
            if (item == null) return false;

            // Delete all media from Cloudinary first
            foreach (var media in item.MediaItems)
            {
                await _cloudinaryService.DeleteAsync(media.PublicId);
            }

            return await _portfolioRepository.DeleteAsync(id);
        }

        public async Task<PortfolioMediaResponseDTO> UploadMediaAsync(Guid portfolioItemId, IFormFile file)
        {
            var item = await _portfolioRepository.GetByIdAsync(portfolioItemId);
            if (item == null) throw new Exception("Portfolio item not found.");

            var (url, publicId) = await _cloudinaryService.UploadAsync(file);

            var isVideo = file.ContentType.StartsWith("video/");
            var media = new PortfolioMedia
            {
                Id = Guid.NewGuid(),
                PortfolioItemId = portfolioItemId,
                Url = url,
                PublicId = publicId,
                MediaType = isVideo ? "video" : "image",
                DisplayOrder = item.MediaItems.Count,
                CreatedAt = DateTime.UtcNow
            };

            await _portfolioRepository.AddMediaAsync(media);

            // Set first image as thumbnail if none exists
            if (item.ThumbnailUrl == null && !isVideo)
            {
                item.ThumbnailUrl = url;
                await _portfolioRepository.UpdateAsync(item);
            }

            return new PortfolioMediaResponseDTO
            {
                Id = media.Id,
                Url = media.Url,
                PublicId = media.PublicId,
                MediaType = media.MediaType,
                DisplayOrder = media.DisplayOrder
            };
        }

        public async Task<bool> DeleteMediaAsync(Guid mediaId)
        {
            var media = await _portfolioRepository.GetMediaByIdAsync(mediaId);
            if (media == null) return false;

            await _cloudinaryService.DeleteAsync(media.PublicId);
            return await _portfolioRepository.DeleteMediaAsync(mediaId);
        }

        // ─── Mapper ──────────────────────────────────────────

        private PortfolioItemResponseDTO MapToDTO(PortfolioItem item)
        {
            return new PortfolioItemResponseDTO
            {
                Id = item.Id,
                Title = item.Title,
                Category = item.Category,
                ThumbnailUrl = item.ThumbnailUrl,
                Content = item.Content,
                ClientName = item.ClientName,
                ProjectId = item.ProjectId,
                IsPublished = item.IsPublished,
                DisplayOrder = item.DisplayOrder,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                MediaItems = item.MediaItems.Select(m => new PortfolioMediaResponseDTO
                {
                    Id = m.Id,
                    Url = m.Url,
                    PublicId = m.PublicId,
                    MediaType = m.MediaType,
                    DisplayOrder = m.DisplayOrder
                }).ToList()
            };
        }
    }
}

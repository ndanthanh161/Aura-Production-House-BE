using Aura.Application.Common;
using Aura.Application.DTOs.Portfolio;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Aura.Application.Services;

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
        var item = PortfolioMapper.ToEntity(request);
        var created = await _portfolioRepository.AddAsync(item);
        return PortfolioMapper.ToDTO(created);
    }

    public async Task<PortfolioItemResponseDTO?> GetByIdAsync(Guid id)
    {
        var item = await _portfolioRepository.GetByIdAsync(id);
        return item == null ? null : PortfolioMapper.ToDTO(item);
    }

    public async Task<IEnumerable<PortfolioItemResponseDTO>> GetPublishedAsync()
    {
        var items = await _portfolioRepository.GetPublishedAsync();
        return items.Select(PortfolioMapper.ToDTO);
    }

    public async Task<PortfolioItemResponseDTO?> UpdateAsync(UpdatePortfolioRequestDTO request)
    {
        var item = await _portfolioRepository.GetByIdAsync(request.Id);
        if (item == null) return null;

        PortfolioMapper.UpdateItem(item, request);

        var updated = await _portfolioRepository.UpdateAsync(item);
        return PortfolioMapper.ToDTO(updated);
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
    public async Task<IEnumerable<PortfolioItemResponseDTO>> GetAllAsync()
    {
        var items = await _portfolioRepository.GetAllAsync();
        return items.Select(PortfolioMapper.ToDTO);
    }

    public async Task<PortfolioMediaResponseDTO> UploadMediaAsync(Guid portfolioItemId, IFormFile file)
    {
        var item = await _portfolioRepository.GetByIdAsync(portfolioItemId);
        if (item == null) throw new Exception(ErrorMessages.PortfolioItemNotFound);

        var (url, publicId) = await _cloudinaryService.UploadAsync(file);
        var isVideo = file.ContentType.StartsWith("video/");
        
        var media = PortfolioMapper.ToMediaEntity(
            portfolioItemId, url, publicId, isVideo ? "video" : "image", item.MediaItems.Count);

        await _portfolioRepository.AddMediaAsync(media);

        // Set first image as thumbnail if none exists
        if (item.ThumbnailUrl == null && !isVideo)
        {
            item.ThumbnailUrl = url;
            await _portfolioRepository.UpdateAsync(item);
        }

        return PortfolioMapper.ToMediaDTO(media);
    }

    public async Task<PortfolioMediaResponseDTO> AddMediaDirectAsync(Guid portfolioItemId, string url, string publicId, string mediaType)
    {
        var item = await _portfolioRepository.GetByIdAsync(portfolioItemId);
        if (item == null) throw new Exception(ErrorMessages.PortfolioItemNotFound);

        var media = PortfolioMapper.ToMediaEntity(
            portfolioItemId, url, publicId, mediaType, item.MediaItems.Count);

        await _portfolioRepository.AddMediaAsync(media);

        // Set first image as thumbnail if none exists
        if (item.ThumbnailUrl == null && mediaType == "image")
        {
            item.ThumbnailUrl = url;
            await _portfolioRepository.UpdateAsync(item);
        }

        return PortfolioMapper.ToMediaDTO(media);
    }

    public object GetUploadSignature(string folder = "portfolio")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var parameters = new Dictionary<string, object>
        {
            { "timestamp", timestamp }
        };

        // Cloudinary signed upload doesn't strictly require resource_type in signature 
        // unless it's explicitly passed as a parameter to the upload call.
        // However, to be safe and clear, we sign only the timestamp.

        var signature = _cloudinaryService.GenerateSignature(parameters);
        var (cloudName, apiKey) = _cloudinaryService.GetCloudSettings();

        return new
        {
            signature,
            timestamp,
            cloudName,
            apiKey,
            folder = $"aura/{folder}"
        };
    }

    public async Task<bool> DeleteMediaAsync(Guid mediaId)
    {
        var media = await _portfolioRepository.GetMediaByIdAsync(mediaId);
        if (media == null) return false;

        await _cloudinaryService.DeleteAsync(media.PublicId);
        return await _portfolioRepository.DeleteMediaAsync(mediaId);
    }
}

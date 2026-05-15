using Aura.Application.DTOs.Portfolio;
using Aura.Domain.Entity;

namespace Aura.Application.Mappers;

public static class PortfolioMapper
{
    public static PortfolioItem ToEntity(CreatePortfolioRequestDTO request)
    {
        return new PortfolioItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Category = request.Category,
            Content = request.Content,
            ClientName = request.ClientName,
            ProjectId = request.ProjectId,
            DisplayOrder = request.DisplayOrder,
            IsHot = request.IsHot,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static PortfolioMedia ToMediaEntity(Guid portfolioItemId, string url, string publicId, string mediaType, int displayOrder)
    {
        return new PortfolioMedia
        {
            Id = Guid.NewGuid(),
            PortfolioItemId = portfolioItemId,
            Url = url,
            PublicId = publicId,
            MediaType = mediaType,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static PortfolioItemResponseDTO ToDTO(PortfolioItem item)
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
            IsHot = item.IsHot,
            DisplayOrder = item.DisplayOrder,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            MediaItems = item.MediaItems?
                .OrderBy(m => m.DisplayOrder)
                .Select(ToMediaDTO).ToList() ?? new List<PortfolioMediaResponseDTO>()
        };
    }

    public static PortfolioMediaResponseDTO ToMediaDTO(PortfolioMedia media)
    {
        return new PortfolioMediaResponseDTO
        {
            Id = media.Id,
            Url = media.Url,
            PublicId = media.PublicId,
            MediaType = media.MediaType,
            DisplayOrder = media.DisplayOrder
        };
    }

    public static void UpdateItem(PortfolioItem item, UpdatePortfolioRequestDTO request)
    {
        item.Title = request.Title;
        item.Category = request.Category;
        item.Content = request.Content;
        item.ClientName = request.ClientName;
        item.ProjectId = request.ProjectId;
        item.DisplayOrder = request.DisplayOrder;
        item.IsHot = request.IsHot;
        item.UpdatedAt = DateTime.UtcNow;
    }
}

using Aura.Application.DTOs.DocumentTemplate;
using Aura.Domain.Entity;
using System;

namespace Aura.Application.Mappers
{
    public static class DocumentTemplateMapper
    {
        public static DocumentTemplate ToEntity(CreateDocumentTemplateDTO request, string fileUrl, string publicId, string fileType)
        {
            return new DocumentTemplate
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                FileUrl = fileUrl,
                PublicId = publicId,
                FileType = fileType,
                IsPublished = request.IsPublished,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static DocumentTemplateResponseDTO ToDTO(DocumentTemplate template)
        {
            return new DocumentTemplateResponseDTO
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                FileUrl = template.FileUrl,
                FileType = template.FileType,
                IsPublished = template.IsPublished,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }

        public static void UpdateEntity(DocumentTemplate template, UpdateDocumentTemplateDTO request)
        {
            template.Title = request.Title;
            template.Description = request.Description;
            template.IsPublished = request.IsPublished;
            template.UpdatedAt = DateTime.UtcNow;
        }
    }
}

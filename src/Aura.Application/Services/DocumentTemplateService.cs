using Aura.Application.DTOs.DocumentTemplate;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aura.Application.Services
{
    public class DocumentTemplateService : IDocumentTemplateService
    {
        private readonly IDocumentTemplateRepository _repository;
        private readonly ICloudinaryService _cloudinaryService;

        public DocumentTemplateService(IDocumentTemplateRepository repository, ICloudinaryService cloudinaryService)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<DocumentTemplateResponseDTO> CreateTemplateAsync(CreateDocumentTemplateDTO request)
        {
            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
            
            // Upload to Cloudinary under folder "templates"
            var uploadResult = await _cloudinaryService.UploadAsync(request.File, "templates");
            
            var template = DocumentTemplateMapper.ToEntity(
                request, 
                uploadResult.Url, 
                uploadResult.PublicId, 
                fileExtension
            );

            var created = await _repository.AddAsync(template);
            return DocumentTemplateMapper.ToDTO(created);
        }

        public async Task<DocumentTemplateResponseDTO?> GetTemplateByIdAsync(Guid id)
        {
            var template = await _repository.GetByIdAsync(id);
            return template == null ? null : DocumentTemplateMapper.ToDTO(template);
        }

        public async Task<IEnumerable<DocumentTemplateResponseDTO>> GetAllTemplatesAsync(bool onlyPublished = false)
        {
            var templates = await _repository.GetAllAsync(onlyPublished);
            return templates.Select(DocumentTemplateMapper.ToDTO);
        }

        public async Task<DocumentTemplateResponseDTO?> UpdateTemplateAsync(UpdateDocumentTemplateDTO request)
        {
            var template = await _repository.GetByIdAsync(request.Id);
            if (template == null) return null;

            DocumentTemplateMapper.UpdateEntity(template, request);
            var updated = await _repository.UpdateAsync(template);
            return DocumentTemplateMapper.ToDTO(updated);
        }

        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            var template = await _repository.GetByIdAsync(id);
            if (template == null) return false;

            // Xóa file trên Cloudinary (Do Word/PDF lưu dưới dạng tệp "raw")
            try
            {
                await _cloudinaryService.DeleteAsync(template.PublicId, "raw");
            }
            catch
            {
                // Bỏ qua lỗi Cloudinary để tiếp tục xóa trong DB
            }

            return await _repository.DeleteAsync(id);
        }
    }
}

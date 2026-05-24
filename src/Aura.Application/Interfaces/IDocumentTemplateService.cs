using Aura.Application.DTOs.DocumentTemplate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aura.Application.Interfaces
{
    public interface IDocumentTemplateService
    {
        Task<DocumentTemplateResponseDTO> CreateTemplateAsync(CreateDocumentTemplateDTO request);
        Task<DocumentTemplateResponseDTO?> GetTemplateByIdAsync(Guid id);
        Task<IEnumerable<DocumentTemplateResponseDTO>> GetAllTemplatesAsync(bool onlyPublished = false);
        Task<DocumentTemplateResponseDTO?> UpdateTemplateAsync(UpdateDocumentTemplateDTO request);
        Task<bool> DeleteTemplateAsync(Guid id);
    }
}

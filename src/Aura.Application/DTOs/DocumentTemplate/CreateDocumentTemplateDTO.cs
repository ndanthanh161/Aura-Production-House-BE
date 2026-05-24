using Microsoft.AspNetCore.Http;

namespace Aura.Application.DTOs.DocumentTemplate
{
    public class CreateDocumentTemplateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile File { get; set; } = null!;
        public bool IsPublished { get; set; }
    }
}

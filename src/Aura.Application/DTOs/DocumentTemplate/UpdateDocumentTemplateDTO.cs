using System;

namespace Aura.Application.DTOs.DocumentTemplate
{
    public class UpdateDocumentTemplateDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublished { get; set; }
    }
}

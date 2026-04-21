namespace Aura.Application.DTOs.Package
{
    public class CreatePackageRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string Features { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
    }
}

namespace Aura.Application.DTOs.Package
{
    public class UpdatePackageRequestDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string Features { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
        public bool IsActive { get; set; }
    }
}

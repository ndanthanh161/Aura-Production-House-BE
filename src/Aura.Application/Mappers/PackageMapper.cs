using Aura.Application.DTOs.Package;
using Aura.Domain.Entity;

namespace Aura.Application.Mappers;

public static class PackageMapper
{
    public static Package ToEntity(CreatePackageRequestDTO request)
    {
        return new Package
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            Benefits = request.Benefits,
            IsPopular = request.IsPopular,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static PackageResponseDTO ToDTO(Package package)
    {
        return new PackageResponseDTO
        {
            Id = package.Id,
            Name = package.Name,
            Price = package.Price,
            Description = package.Description,
            Benefits = package.Benefits,
            IsPopular = package.IsPopular,
            IsActive = package.IsActive,
            CreatedAt = package.CreatedAt,
            UpdatedAt = package.UpdatedAt
        };
    }

    public static void UpdatePackage(Package package, UpdatePackageRequestDTO request)
    {
        package.Name = request.Name;
        package.Price = request.Price;
        package.Description = request.Description;
        package.Benefits = request.Benefits;
        package.IsPopular = request.IsPopular;
        package.IsActive = request.IsActive;
        package.UpdatedAt = DateTime.UtcNow;
    }
}

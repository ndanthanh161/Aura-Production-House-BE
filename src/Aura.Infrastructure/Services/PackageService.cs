using Aura.Application.DTOs.Package;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;

namespace Aura.Infrastructure.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;

        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<PackageResponseDTO> CreatePackageAsync(CreatePackageRequestDTO request)
        {
            var package = new Package
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Description = request.Description,
                Features = request.Features,
                IsPopular = request.IsPopular,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdPackage = await _packageRepository.AddAsync(package);
            return MapToDTO(createdPackage);
        }

        public async Task<PackageResponseDTO?> GetPackageByIdAsync(Guid id)
        {
            var package = await _packageRepository.GetByIdAsync(id);
            return package == null ? null : MapToDTO(package);
        }

        public async Task<IEnumerable<PackageResponseDTO>> GetAllPackagesAsync(bool onlyActive = true)
        {
            var packages = await _packageRepository.GetAllAsync(onlyActive);
            return packages.Select(MapToDTO);
        }

        public async Task<PackageResponseDTO?> UpdatePackageAsync(UpdatePackageRequestDTO request)
        {
            var package = await _packageRepository.GetByIdAsync(request.Id);
            if (package == null) return null;

            package.Name = request.Name;
            package.Price = request.Price;
            package.Description = request.Description;
            package.Features = request.Features;
            package.IsPopular = request.IsPopular;
            package.IsActive = request.IsActive; // Admin có quyền Set Active/Inactive
            package.UpdatedAt = DateTime.UtcNow;

            var updatedPackage = await _packageRepository.UpdateAsync(package);
            return MapToDTO(updatedPackage);
        }

        public async Task<bool> DeletePackageAsync(Guid id)
        {
            return await _packageRepository.DeleteAsync(id);
        }

        // Helper private function mapping DB Entity to Data Transfer Object
        private PackageResponseDTO MapToDTO(Package package)
        {
            return new PackageResponseDTO
            {
                Id = package.Id,
                Name = package.Name,
                Price = package.Price,
                Description = package.Description,
                Features = package.Features,
                IsPopular = package.IsPopular,
                IsActive = package.IsActive,
                CreatedAt = package.CreatedAt,
                UpdatedAt = package.UpdatedAt
            };
        }
    }
}

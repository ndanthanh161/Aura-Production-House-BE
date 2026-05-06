using Aura.Application.DTOs.Package;

namespace Aura.Application.Interfaces
{
    public interface IPackageService
    {
        Task<PackageResponseDTO> CreatePackageAsync(CreatePackageRequestDTO request);
        Task<PackageResponseDTO?> GetPackageByIdAsync(Guid id);
        Task<IEnumerable<PackageResponseDTO>> GetAllPackagesAsync(bool onlyActive = true);
        Task<PackageResponseDTO?> UpdatePackageAsync(UpdatePackageRequestDTO request);
        Task<bool> DeletePackageAsync(Guid id);
    }
}

using Aura.Application.DTOs.Package;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;

namespace Aura.Application.Services;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;

    public PackageService(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public async Task<PackageResponseDTO> CreatePackageAsync(CreatePackageRequestDTO request)
    {
        var package = PackageMapper.ToEntity(request);
        var createdPackage = await _packageRepository.AddAsync(package);
        return PackageMapper.ToDTO(createdPackage);
    }

    public async Task<PackageResponseDTO?> GetPackageByIdAsync(Guid id)
    {
        var package = await _packageRepository.GetByIdAsync(id);
        return package == null ? null : PackageMapper.ToDTO(package);
    }

    public async Task<IEnumerable<PackageResponseDTO>> GetAllPackagesAsync(bool onlyActive = true)
    {
        var packages = await _packageRepository.GetAllAsync(onlyActive);
        return packages.Select(PackageMapper.ToDTO);
    }

    public async Task<PackageResponseDTO?> UpdatePackageAsync(UpdatePackageRequestDTO request)
    {
        var package = await _packageRepository.GetByIdAsync(request.Id);
        if (package == null) return null;

        PackageMapper.UpdatePackage(package, request);

        var updatedPackage = await _packageRepository.UpdateAsync(package);
        return PackageMapper.ToDTO(updatedPackage);
    }

    public async Task<bool> DeletePackageAsync(Guid id)
    {
        return await _packageRepository.DeleteAsync(id);
    }
}

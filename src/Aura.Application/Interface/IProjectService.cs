using Aura.Application.DTOs.Project;
using Aura.Domain.Enum;

namespace Aura.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponseDTO> CreateProjectAsync(CreateProjectRequestDTO request);
        Task<ProjectResponseDTO?> GetProjectByIdAsync(Guid id);
        Task<IEnumerable<ProjectResponseDTO>> GetAllProjectsAsync();
        Task<ProjectResponseDTO?> UpdateProjectAsync(UpdateProjectRequestDTO request);
        Task<bool> UpdateProjectStatusAsync(Guid id, ProjectStatus status);
    }
}

using Aura.Application.DTOs.Project;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;

namespace Aura.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IPackageRepository _packageRepository; // Gọi chéo Repo Package

        public ProjectService(IProjectRepository projectRepository, IPackageRepository packageRepository)
        {
            _projectRepository = projectRepository;
            _packageRepository = packageRepository;
        }

        public async Task<ProjectResponseDTO> CreateProjectAsync(CreateProjectRequestDTO request)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null || !package.IsActive)
            {
                throw new Exception("Bản ghi Package không tồn tại hoặc đã ngừng cung cấp.");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ClientId = request.ClientId,
                PackageId = request.PackageId,

                // Gán giá Revenue của Project dựa theo giá chuẩn Menu (Price) của Package
                Revenue = package.Price,

                Status = ProjectStatus.PreProduction, // Chờ thanh toán
                Deadline = DateTime.UtcNow.AddDays(7), // Mặc định deadline 7 ngày
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectRepository.AddAsync(project);
            return MapToDTO(createdProject);
        }

        public async Task<ProjectResponseDTO?> GetProjectByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            return project == null ? null : MapToDTO(project);
        }

        public async Task<IEnumerable<ProjectResponseDTO>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            return projects.Select(MapToDTO);
        }

        public async Task<ProjectResponseDTO?> UpdateProjectAsync(UpdateProjectRequestDTO request)
        {
            var project = await _projectRepository.GetByIdAsync(request.Id);
            if (project == null) return null;

            project.Name = request.Name;
            project.StaffId = request.StaffId;
            project.Status = request.Status;
            project.Revenue = request.Revenue; // Update lại Doanh thu thực tế nếu phát sinh
            project.Deadline = request.Deadline;
            project.Description = request.Description;
            project.UpdatedAt = DateTime.UtcNow;

            var updatedProject = await _projectRepository.UpdateAsync(project);
            return MapToDTO(updatedProject);
        }

        public async Task<bool> UpdateProjectStatusAsync(Guid id, ProjectStatus status)
        {
            return await _projectRepository.UpdateStatusAsync(id, status);
        }

        private ProjectResponseDTO MapToDTO(Project project)
        {
            return new ProjectResponseDTO
            {
                Id = project.Id,
                Name = project.Name,
                ClientId = project.ClientId,
                PackageId = project.PackageId,
                StaffId = project.StaffId == Guid.Empty ? null : project.StaffId,
                Status = project.Status,
                Revenue = project.Revenue,
                Deadline = project.Deadline,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
    }
}

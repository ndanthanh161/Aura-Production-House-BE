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
                Deposit = request.Deposit, // Gán tiền cọc thực tế khách hàng đã thanh toán

                // Tự động snapshot toàn bộ Benefits từ Package → không cần Customer nhập thủ công
                Benefits = new List<string>(package.Benefits),

                Status = ProjectStatus.InProduction, // Đang thực hiện (sau khi thanh toán)
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
            project.Deposit = request.Deposit; // Update tiền cọc
            project.Deadline = request.Deadline;
            project.Description = request.Description;
            project.UpdatedAt = DateTime.UtcNow;
            // Lưu ý: KHÔNG update Benefits ở đây vì đây là snapshot đã cam kết với customer

            var updatedProject = await _projectRepository.UpdateAsync(project);
            return MapToDTO(updatedProject);
        }

        public async Task<bool> UpdateProjectStatusAsync(Guid id, ProjectStatus status)
        {
            return await _projectRepository.UpdateStatusAsync(id, status);
        }

        public async Task<bool> UpdateProjectStaffAsync(Guid id, Guid staffId)
        {
            return await _projectRepository.UpdateStaffAsync(id, staffId);
        }

        public async Task<IEnumerable<ProjectResponseDTO>> GetSchedulesAsync(Guid? clientId, Guid? staffId, DateTime? from, DateTime? to)
        {
            var projects = await _projectRepository.GetSchedulesAsync(clientId, staffId, from, to);
            return projects.Select(MapToDTO);
        }

        public async Task<SlotAvailabilityResponseDTO> CheckSlotAvailabilityAsync(DateTime date, int maxSlotsPerDay = 3)
        {
            var booked = (await _projectRepository.GetBookedOnDateAsync(date)).ToList();

            return new SlotAvailabilityResponseDTO
            {
                Date = date.Date,
                BookedCount = booked.Count,
                IsAvailable = booked.Count < maxSlotsPerDay,
                BookedProjectIds = booked.Select(p => p.Id)
            };
        }

        public async Task<ProjectResponseDTO?> RescheduleAsync(RescheduleRequestDTO request)
        {
            if (request.NewShootingDate.ToUniversalTime() < DateTime.UtcNow)
                throw new ArgumentException("Ngày chụp mới không được trong quá khứ.");

            var updated = await _projectRepository.RescheduleAsync(
                request.ProjectId, request.NewShootingDate.ToUniversalTime());

            return updated == null ? null : MapToDTO(updated);
        }

        public async Task<bool> CancelProjectAsync(Guid projectId)
        {
            return await _projectRepository.CancelAsync(projectId);
        }

        private ProjectResponseDTO MapToDTO(Project project)
        {
            return new ProjectResponseDTO
            {
                Id = project.Id,
                Name = project.Name,
                ClientId = project.ClientId,
                ClientName = project.Client?.FullName ?? string.Empty,
                PackageId = project.PackageId,
                PackageName = project.Package?.Name ?? string.Empty,
                Deposit = project.Deposit,
                StaffId = project.StaffId == Guid.Empty ? null : project.StaffId,
                StaffName = project.Staff?.FullName,
                Status = project.Status,
                Revenue = project.Revenue,
                Benefits = project.Benefits,
                Deadline = project.Deadline,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
    }
}

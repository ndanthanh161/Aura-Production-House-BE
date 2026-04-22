using Aura.Application.DTOs.User;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;

namespace Aura.Infrastructure.Services
{
    public class PhotographerService : IPhotographerService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;

        public PhotographerService(
            IUserRepository userRepository,
            IProjectRepository projectRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
        }

        // ─── Lấy tất cả photographer (Staff HOẶC Photographer role) ───────────
        public async Task<IEnumerable<UserResponseDTO>> GetAllPhotographersAsync()
        {
            var staff       = await _userRepository.GetAllByRoleAsync("Staff");
            var photogs     = await _userRepository.GetAllByRoleAsync("Photographer");
            return staff.Concat(photogs).Select(MapToDTO);
        }

        // ─── Chi tiết photographer ─────────────────────────────────────────────
        public async Task<UserResponseDTO?> GetPhotographerByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !IsPhotographerRole(user.Role.Name)) return null;
            return MapToDTO(user);
        }

        // ─── Cập nhật thông tin photographer ──────────────────────────────────
        public async Task<UserResponseDTO?> UpdatePhotographerAsync(UpdateUserRequestDTO request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null || !IsPhotographerRole(user.Role.Name)) return null;

            user.FullName  = request.FullName;
            user.Phone     = request.Phone;
            user.Avatar    = request.Avatar;
            user.UpdatedAt = DateTime.UtcNow;

            var updated = await _userRepository.UpdateAsync(user);
            return MapToDTO(updated);
        }

        // ─── Phân công photographer cho dự án ─────────────────────────────────
        public async Task<bool> AssignToProjectAsync(Guid photographerId, Guid projectId)
        {
            var user = await _userRepository.GetByIdAsync(photographerId);
            if (user == null || !IsPhotographerRole(user.Role.Name)) return false;

            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return false;

            project.StaffId   = photographerId;
            project.UpdatedAt = DateTime.UtcNow;
            await _projectRepository.UpdateAsync(project);
            return true;
        }

        // ─── Vô hiệu hóa photographer ─────────────────────────────────────────
        public async Task<bool> DeactivatePhotographerAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !IsPhotographerRole(user.Role.Name)) return false;

            if (!user.FullName.StartsWith("[Da nghi]"))
                user.FullName = $"[Da nghi] {user.FullName}";

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        // ─── Helper: chấp nhận cả Staff và Photographer ───────────────────────
        private static bool IsPhotographerRole(string roleName) =>
            roleName == "Staff" || roleName == "Photographer";

        // ─── Helper: map sang DTO ─────────────────────────────────────────────
        private static UserResponseDTO MapToDTO(User user) => new()
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Phone     = user.Phone,
            Avatar    = user.Avatar,
            Role      = user.Role.Name,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

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
        private readonly IRoleRepository _roleRepository;

        public PhotographerService(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
        }

        // ─── Lấy tất cả photographer (CHỈ role Photographer) ───────────────
        public async Task<IEnumerable<UserResponseDTO>> GetAllPhotographersAsync()
        {
            var photogs = await _userRepository.GetAllByRoleAsync("Photographer");
            return photogs.Select(MapToDTO);
        }

        // ─── Chi tiết photographer ─────────────────────────────────────────────
        public async Task<UserResponseDTO?> GetPhotographerByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !IsPhotographerRole(user.Role.Name)) return null;
            return MapToDTO(user);
        }

        public async Task<UserResponseDTO> CreatePhotographerAsync(CreatePhotographerRequestDTO request)
        {
            // 1. Kiểm tra email tồn tại
            if (await _userRepository.ExistsByEmailAsync(request.Email))
                throw new ArgumentException("Email đã tồn tại.");

            // 2. Tìm Role (Staff hoặc Photographer)
            var role = await _roleRepository.GetByNameAsync(request.Role);
            if (role == null) throw new InvalidOperationException($"Role '{request.Role}' not found.");

            // 3. Tạo User mới
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email.ToLower().Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _userRepository.CreateAsync(user);
            return MapToDTO(created);
        }

        // ─── Cập nhật thông tin photographer ──────────────────────────────────
        public async Task<UserResponseDTO?> UpdatePhotographerAsync(UpdateUserRequestDTO request)

        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null || !IsPhotographerRole(user.Role.Name)) return null;

            user.FullName       = request.FullName;
            user.Phone          = request.Phone;
            user.Avatar         = request.Avatar;
            user.Bio            = request.Bio;
            user.Specialization = request.Specialization;
            user.UpdatedAt      = DateTime.UtcNow;

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

        // ─── Vô hiệu hóa photographer (Soft Delete) ──────────────────────────
        public async Task<bool> DeactivatePhotographerAsync(Guid id)
        {
            return await _userRepository.DeactivateAsync(id);
        }

        // ─── Helper: chỉ chấp nhận Photographer ──────────────────────────────
        private static bool IsPhotographerRole(string roleName) =>
            roleName == "Photographer";

        // ─── Helper: map sang DTO ─────────────────────────────────────────────
        private static UserResponseDTO MapToDTO(User user) => new()
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Phone          = user.Phone,
            Avatar         = user.Avatar,
            Bio            = user.Bio,
            Specialization = user.Specialization,
            Role           = user.Role.Name,
            IsActive       = user.IsActive,
            CreatedAt      = user.CreatedAt,
            UpdatedAt      = user.UpdatedAt
        };
    }
}

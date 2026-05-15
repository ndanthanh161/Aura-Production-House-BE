using Aura.Application.Common;
using Aura.Application.DTOs.User;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;

namespace Aura.Application.Services;

public class PhotographerService : IPhotographerService
{
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public PhotographerService(
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    // ─── Lấy tất cả photographer (CHỈ role Photographer) ───────────────
    public async Task<IEnumerable<UserResponseDTO>> GetAllPhotographersAsync()
    {
        var photogs = await _userRepository.GetAllByRoleAsync("Photographer");
        return photogs.Select(UserMapper.ToDTO);
    }

    // ─── Chi tiết photographer ─────────────────────────────────────────────
    public async Task<UserResponseDTO?> GetPhotographerByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || !IsPhotographerRole(user.Role.Name)) return null;
        return UserMapper.ToDTO(user);
    }

    public async Task<UserResponseDTO> CreatePhotographerAsync(CreatePhotographerRequestDTO request)
    {
        // 1. Kiểm tra email tồn tại
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new ArgumentException(ErrorMessages.DuplicateEmail);

        // 2. Tìm Role
        var role = await _roleRepository.GetByNameAsync(request.Role);
        if (role == null) throw new InvalidOperationException($"Role '{request.Role}' not found.");

        // 3. Tạo User mới thông qua Mapper
        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        var user = UserMapper.ToEntity(request, role.Id, hashedPassword);

        var created = await _userRepository.CreateAsync(user);
        return UserMapper.ToDTO(created);
    }

    // ─── Cập nhật thông tin photographer ──────────────────────────────────
    public async Task<UserResponseDTO?> UpdatePhotographerAsync(UpdateUserRequestDTO request)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        if (user == null || !IsPhotographerRole(user.Role.Name)) return null;

        UserMapper.UpdateUser(user, request);

        var updated = await _userRepository.UpdateAsync(user);
        return UserMapper.ToDTO(updated);
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
}

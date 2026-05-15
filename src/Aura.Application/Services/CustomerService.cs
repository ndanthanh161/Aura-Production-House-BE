using Aura.Application.DTOs.User;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;

namespace Aura.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUserRepository _userRepository;

    public CustomerService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // ─── Lấy tất cả khách hàng (User) ─────────────────────────────────
    public async Task<IEnumerable<UserResponseDTO>> GetAllCustomersAsync()
    {
        var customers = await _userRepository.GetAllByRoleAsync("User");
        return customers.Select(UserMapper.ToDTO);
    }

    // ─── Chi tiết khách hàng ───────────────────────────────────────────
    public async Task<UserResponseDTO?> GetCustomerByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role.Name != "User") return null;
        return UserMapper.ToDTO(user);
    }

    // ─── Cập nhật thông tin khách hàng ────────────────────────────────
    public async Task<UserResponseDTO?> UpdateCustomerAsync(UpdateUserRequestDTO request)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        if (user == null || user.Role.Name != "User") return null;

        UserMapper.UpdateUser(user, request);


        var updated = await _userRepository.UpdateAsync(user);
        return UserMapper.ToDTO(updated);
    }

    public async Task<bool> DeactivateCustomerAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role.Name != "User") return false;

        return await _userRepository.DeactivateAsync(id);
    }
}

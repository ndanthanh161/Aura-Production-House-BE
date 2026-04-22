using Aura.Application.DTOs.User;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;

namespace Aura.Infrastructure.Services
{
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
            return customers.Select(MapToDTO);
        }

        // ─── Chi tiết khách hàng ───────────────────────────────────────────
        public async Task<UserResponseDTO?> GetCustomerByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.Role.Name != "User") return null;
            return MapToDTO(user);
        }

        // ─── Cập nhật thông tin khách hàng ────────────────────────────────
        public async Task<UserResponseDTO?> UpdateCustomerAsync(UpdateUserRequestDTO request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null || user.Role.Name != "User") return null;

            user.FullName  = request.FullName;
            user.Phone     = request.Phone;
            user.Avatar    = request.Avatar;
            user.UpdatedAt = DateTime.UtcNow;

            var updated = await _userRepository.UpdateAsync(user);
            return MapToDTO(updated);
        }

        // ─── Helper ───────────────────────────────────────────────────────
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

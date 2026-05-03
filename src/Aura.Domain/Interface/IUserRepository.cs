using Aura.Domain.Entity;

namespace Aura.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User> CreateAsync(User user);

        /// <summary>Lấy tất cả user theo tên role (Admin, Staff, User)</summary>
        Task<IEnumerable<User>> GetAllByRoleAsync(string roleName);

        /// <summary>Cập nhật thông tin user</summary>
        Task<User> UpdateAsync(User user);

        /// <summary>Vô hiệu hóa user (Soft Delete)</summary>
        Task<bool> DeactivateAsync(Guid id);
    }
}

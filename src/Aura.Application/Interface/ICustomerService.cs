using Aura.Application.DTOs.User;

namespace Aura.Application.Interfaces
{
    public interface ICustomerService
    {
        /// <summary>Lấy danh sách tất cả khách hàng (role = User)</summary>
        Task<IEnumerable<UserResponseDTO>> GetAllCustomersAsync();

        /// <summary>Lấy chi tiết khách hàng theo Id</summary>
        Task<UserResponseDTO?> GetCustomerByIdAsync(Guid id);

        /// <summary>Cập nhật thông tin khách hàng</summary>
        Task<UserResponseDTO?> UpdateCustomerAsync(UpdateUserRequestDTO request);

        /// <summary>Vô hiệu hóa khách hàng (Soft Delete)</summary>
        Task<bool> DeactivateCustomerAsync(Guid id);
    }
}

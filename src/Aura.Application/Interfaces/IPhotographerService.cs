using Aura.Application.DTOs.User;

namespace Aura.Application.Interfaces
{
    public interface IPhotographerService
    {
        /// <summary>Lấy danh sách tất cả nhân viên (Staff)</summary>
        Task<IEnumerable<UserResponseDTO>> GetAllPhotographersAsync();

        /// <summary>Lấy chi tiết một photographer theo Id</summary>
        Task<UserResponseDTO?> GetPhotographerByIdAsync(Guid id);

        /// <summary>Tạo mới một photographer (Admin only)</summary>
        Task<UserResponseDTO> CreatePhotographerAsync(CreatePhotographerRequestDTO request);

        /// <summary>Cập nhật thông tin photographer</summary>
        Task<UserResponseDTO?> UpdatePhotographerAsync(UpdateUserRequestDTO request);

        /// <summary>Admin phân công photographer cho dự án</summary>
        Task<bool> AssignToProjectAsync(Guid photographerId, Guid projectId);

        /// <summary>Xóa/vô hiệu hóa photographer (soft delete qua lịch sử)</summary>
        Task<bool> DeactivatePhotographerAsync(Guid id);
    }
}

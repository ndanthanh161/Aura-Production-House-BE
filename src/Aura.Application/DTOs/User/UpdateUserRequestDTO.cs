using System.ComponentModel.DataAnnotations;

namespace Aura.Application.DTOs.User
{
    /// <summary>Cập nhật thông tin user (Admin dùng)</summary>
    public class UpdateUserRequestDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        public string? Avatar { get; set; }

        [StringLength(1000, ErrorMessage = "Giới thiệu không quá 1000 ký tự")]
        public string? Bio { get; set; }

        [StringLength(200, ErrorMessage = "Chuyên môn không quá 200 ký tự")]
        public string? Specialization { get; set; }
    }
}

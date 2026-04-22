namespace Aura.Application.DTOs.User
{
    /// <summary>Cập nhật thông tin user (Admin dùng)</summary>
    public class UpdateUserRequestDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
    }
}

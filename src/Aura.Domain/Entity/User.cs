namespace Aura.Domain.Entity
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; } // Giới thiệu bản thân
        public string? Specialization { get; set; } // Chuyên môn (ví dụ: Chụp ảnh cưới, Sự kiện)
        public bool IsActive { get; set; } = true;
        public bool IsVip { get; set; } = false;
        public DateTime? VipExpireAt { get; set; }
        public bool HasClaimedFreeMembership { get; set; } = false;
        public DateTime? FreeMembershipClaimedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Role Role { get; set; } = null!;
        public ICollection<Project> ClientProjects { get; set; } = new List<Project>();
        public ICollection<Project> StaffProjects { get; set; } = new List<Project>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}

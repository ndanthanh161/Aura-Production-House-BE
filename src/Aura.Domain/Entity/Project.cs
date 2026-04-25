using Aura.Domain.Enum;

namespace Aura.Domain.Entity
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public Guid? StaffId { get; set; }
        public Guid PackageId { get; set; }
        public ProjectStatus Status { get; set; }
        public decimal Revenue { get; set; }
        public decimal Deposit { get; set; } // Tiền cọc
        public DateTime Deadline { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// Snapshot danh sách lợi ích tại thời điểm customer mua package.
        /// Không bị ảnh hưởng nếu Package sau này bị cập nhật hay xóa.
        /// </summary>
        public List<string> Benefits { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User Client { get; set; } = null!;
        public User? Staff { get; set; }
        public Package Package { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();
    }
}


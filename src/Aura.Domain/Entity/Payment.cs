using Aura.Domain.Enum;

namespace Aura.Domain.Entity
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        public Project Project { get; set; } = null!;
    }
}

using Aura.Domain.Entity;
using Aura.Domain.Enum;

namespace Aura.Application.Mappers;

public static class PaymentMapper
{
    public static Payment ToEntity(Project project, decimal amount, string transactionId)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            UserId = project.ClientId,
            ProjectId = project.Id,
            Amount = amount,
            Currency = "VND",
            TotalAmount = amount,
            OrderCode = $"AURA-{DateTime.Now:yyyyMMdd}-{transactionId.Substring(Math.Max(0, transactionId.Length - 4))}",
            PaymentMethod = PaymentMethod.VietQR,
            Gateway = "SePay",
            Status = PaymentStatus.Completed,
            TransactionId = transactionId,
            Note = $"Thanh toan cho du an {project.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

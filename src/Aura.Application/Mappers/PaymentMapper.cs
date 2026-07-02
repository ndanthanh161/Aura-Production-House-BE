using Aura.Application.Services;
using Aura.Domain.Entity;
using Aura.Domain.Enum;

namespace Aura.Application.Mappers;

public static class PaymentMapper
{
    public static Payment ToEntity(Project project, PaymentInstallment installment, decimal amount, string transactionId)
    {
        var txIdSafe = transactionId ?? string.Empty;
        var vnTime = DateTime.UtcNow.AddHours(7); // Đồng bộ múi giờ Việt Nam (UTC+7)

        return new Payment
        {
            Id = Guid.NewGuid(),
            UserId = project.ClientId,
            ProjectId = project.Id,
            Amount = amount,
            Currency = "VND",
            TotalAmount = amount,
            InstallmentNumber = installment.InstallmentNumber,
            InstallmentPercentage = installment.Percentage,
            RequiredAmount = installment.Amount,
            OrderCode = $"AURA-{vnTime:yyyyMMdd}-{txIdSafe.Substring(Math.Max(0, txIdSafe.Length - 4))}",
            PaymentMethod = PaymentMethod.VietQR,
            Gateway = "SePay",
            Status = PaymentStatus.Completed,
            TransactionId = transactionId,
            Note = $"Thanh toan dot {installment.InstallmentNumber} cho du an {project.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

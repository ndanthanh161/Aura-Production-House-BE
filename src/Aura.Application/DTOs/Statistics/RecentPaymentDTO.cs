namespace Aura.Application.DTOs.Statistics;

public class RecentPaymentDTO
{
    public Guid PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
}

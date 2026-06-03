using System;

namespace Aura.Application.DTOs.Payment
{
    public class ManualConfirmPaymentDTO
    {
        public Guid ProjectId { get; set; }
        public decimal TransferAmount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}

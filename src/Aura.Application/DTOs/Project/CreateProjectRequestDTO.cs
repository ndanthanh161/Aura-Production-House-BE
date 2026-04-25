namespace Aura.Application.DTOs.Project
{
    public class CreateProjectRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public Guid ClientId { get; set; } // Khách hàng nào mua
        public Guid PackageId { get; set; } // Hợp đồng dựa trên Gói nguyên bản nào
        public decimal Deposit { get; set; }
        public string? Description { get; set; }
    }
}

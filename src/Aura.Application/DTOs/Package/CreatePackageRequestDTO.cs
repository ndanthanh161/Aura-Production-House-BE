namespace Aura.Application.DTOs.Package
{
    public class CreatePackageRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách lợi ích chi tiết. VD: ["Kho template đa nền tảng", "Kịch bản quay", ...]
        /// </summary>
        public List<string> Benefits { get; set; } = new List<string>();

        public bool IsPopular { get; set; }
        public bool IsFreeMembershipOfferEnabled { get; set; }
    }
}


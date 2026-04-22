namespace Aura.Application.DTOs.Booking;
public class SlotAvailabilityResponseDTO
{
    public DateTime Date { get; set; }

    /// <summary>Số lịch chụp đã được đặt trong ngày này</summary>
    public int BookedCount { get; set; }
    public bool IsAvailable { get; set; }

    /// <summary>Danh sách projectId đã đặt slot ngày này (để staff tham khảo)</summary>
    public IEnumerable<Guid> BookedProjectIds { get; set; } = new List<Guid>();
}


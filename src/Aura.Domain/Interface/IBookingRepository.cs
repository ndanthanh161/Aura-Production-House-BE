using Aura.Domain.Entity;
using Aura.Domain.Enum;

namespace Aura.Domain.Interfaces
{
    public interface IBookingRepository
    {
        /// <summary>Lấy danh sách lịch chụp. Lọc theo clientId (customer tự xem) hoặc staffId, khoảng ngày.</summary>
        Task<IEnumerable<Project>> GetSchedulesAsync(Guid? clientId, Guid? staffId, DateTime? from, DateTime? to);

        /// <summary>Lấy số lượng booking đã có trong một ngày cụ thể (status Scheduled/InProduction)</summary>
        Task<IEnumerable<Project>> GetBookedOnDateAsync(DateTime date);

        /// <summary>Đổi lịch: cập nhật Deadline + Status → Scheduled. clientId != null thì kiểm tra ownership.</summary>
        Task<Project?> RescheduleAsync(Guid projectId, DateTime newShootingDate, Guid? clientId = null);

        /// <summary>Hủy lịch: set Status → Cancelled. clientId != null thì kiểm tra ownership.</summary>
        Task<bool> CancelAsync(Guid projectId, Guid? clientId = null);

        /// <summary>Lấy trạng thái booking của một project. clientId != null thì kiểm tra ownership.</summary>
        Task<Project?> GetBookingStatusAsync(Guid projectId, Guid? clientId = null);
    }
}
